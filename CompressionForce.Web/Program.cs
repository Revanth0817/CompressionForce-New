using CompressionForce.Data;
using CompressionForce.Domain.Validation;
using CompressionForce.Services;
using CompressionForce.Services.Audit;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.ModelBinding;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Rotativa.AspNetCore;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// CONFIGURATION
// -----------------------------------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Recipe validation JSON
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "recipe-validation.json"),
    optional: false,
    reloadOnChange: true
);

// -----------------------------------------------------------------------------
// SERVICES
// -----------------------------------------------------------------------------

// Http Context
builder.Services.AddHttpContextAccessor();

// PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// -------------------- VALIDATION --------------------
builder.Services.Configure<RecipeValidationConfig>(builder.Configuration);
builder.Services.AddSingleton<IRecipeValidationConfigProvider, JsonRecipeValidationConfigProvider>();
builder.Services.AddScoped<ConfigRecipeValidator>();
builder.Services.AddScoped<IRecipeValidator, RecipeRulesValidator>();
builder.Services.AddScoped<LookupRecipeValidator>();

// -------------------- APPLICATION SERVICES --------------------
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IPlcStatusService, PlcStatusService>();
builder.Services.AddScoped<AutoTareService>();
builder.Services.AddScoped<CompressionForce.Web.Authorization.PrivilegeService>();

// -------------------- AUDIT --------------------
builder.Services.AddScoped<AuditLogger>();

// -------------------- MVC --------------------
builder.Services
    .AddControllersWithViews(options =>
    {
        options.ModelBinderProviders.Insert(0, new RecipeParameterModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .AddSessionStateTempDataProvider();

// -------------------- SESSION --------------------
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CompressionForce.Session";
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// MIDDLEWARE PIPELINE
// -----------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// browser navigations that end in 403/404/... get the friendly error page instead of a blank screen
app.UseStatusCodePages(async context =>
{
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;

    if (HttpMethods.IsGet(request.Method) &&
        request.Headers.Accept.ToString().Contains("text/html") &&
        !request.Path.StartsWithSegments("/Home/Error"))
    {
        response.Redirect("/Home/Error?code=" + response.StatusCode);
        await Task.CompletedTask;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Session MUST be before custom middleware
app.UseSession();

// -----------------------------------------------------------------------------
// APPLICATION TIMEOUT MIDDLEWARE
// -----------------------------------------------------------------------------
app.Use(async (context, next) =>
{
    var username = context.Session.GetString("UserName");

    if (!string.IsNullOrEmpty(username))
    {
        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        var settings = await db.SecuritySettings.FirstOrDefaultAsync();

        // the page script (layout) logs the user out after this many idle minutes (0 = never)
        context.Session.SetInt32("AppTimeout",
            settings != null && settings.ApplicationTimeoutMinutes > 0 ? settings.ApplicationTimeoutMinutes : 0);

        if (settings != null && settings.ApplicationTimeoutMinutes > 0)
        {
            var lastActivityStr = context.Session.GetString("LastActivity");

            if (!string.IsNullOrEmpty(lastActivityStr) &&
                DateTime.TryParse(lastActivityStr, out var lastActivity))
            {
                var idleMinutes = (DateTime.UtcNow - lastActivity).TotalMinutes;

                if (idleMinutes > settings.ApplicationTimeoutMinutes)
                {
                    context.Session.Clear();
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            context.Session.SetString("LastActivity", DateTime.UtcNow.ToString("O"));
        }
    }

    await next();
});

// -----------------------------------------------------------------------------
// LOGIN GUARD – every page/API except the Account pages requires a logged-in session
// -----------------------------------------------------------------------------
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isPublic =
        path.StartsWithSegments("/Account") ||
        path.StartsWithSegments("/Home/Error");

    if (!isPublic && string.IsNullOrEmpty(context.Session.GetString("UserName")))
    {
        var wantsHtml = context.Request.Method == HttpMethods.Get &&
                        context.Request.Headers.Accept.ToString().Contains("text/html");
        if (wantsHtml)
            context.Response.Redirect("/Account/Login");
        else
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    await next();
});

// Authentication (if enabled later)
// app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------------
// ROUTES
// -----------------------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}"
);

// -----------------------------------------------------------------------------
// ROTATIVA (PDF)
// -----------------------------------------------------------------------------
RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa"
);

// -----------------------------------------------------------------------------
// DEFAULT ADMIN – created on startup when the configured account does not exist.
// Credentials come from the "DefaultAdmin" section (set the password with user-secrets or an environment variable).
// The account must change its password at first login (expiry date is in the past).
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var section = app.Configuration.GetSection("DefaultAdmin");
        var userName = section["UserName"] ?? "admin";
        var email = (section["Email"] ?? "admin@localhost").Trim().ToLower();
        var configuredPassword = section["Password"];
        // never ship a known password: without a configured one a random one is generated and logged once
        var password = !string.IsNullOrEmpty(configuredPassword)
            ? configuredPassword
            : "Aa1!" + Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(9)).Replace('+', 'x').Replace('/', 'y');

        // the privilege system relies on these two groups
        foreach (var group in new[] { "Admin", "User" })
        {
            if (!await db.UserGroups.AnyAsync(g => g.Name == group))
                db.UserGroups.Add(new CompressionForce.Domain.Entities.UserGroup { Name = group });
        }

        if (!await db.UserManagements.AnyAsync(u => u.ERname == userName || u.ERemail == email))
        {
            db.UserManagements.Add(new CompressionForce.Domain.Entities.UserManagement
            {
                ERname = userName,
                ERemail = email,
                ERpassword = BCrypt.Net.BCrypt.HashPassword(password),
                ERlevel = "Admin",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMinutes(-1), // forces a password change at first login
                FailedLoginAttempts = 0
            });
            logger.LogWarning("Default admin account '{UserName}' created - change its password at first login.{Generated}", userName, string.IsNullOrEmpty(configuredPassword) ? $" Generated password: {password}" : "");
        }

        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Could not seed the default admin account.");
    }
}

app.Run();

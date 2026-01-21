using CompressionForce.Data;
using CompressionForce.Domain.Calibration;
using CompressionForce.Domain.PLC;
using CompressionForce.Domain.PLC;
using CompressionForce.Domain.Validation;
using CompressionForce.Integrations.PLC.Modbus;
using CompressionForce.Services;


using CompressionForce.Services;
using CompressionForce.Services.Audit;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.Hubs;
using CompressionForce.Web.ModelBinding;
using CompressionForce.Web.Services;
using MathNet.Numerics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rotativa.AspNetCore;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;



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

// -------------------- AUDIT --------------------
builder.Services.AddScoped<AuditLogger>();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IServoStatusPublisher, SignalRServoStatusPublisher>();



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


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CompressionForce.Session";
});


builder.Services.AddSingleton<IPlcProtocol>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var ip = config["Plc:Ip"];
    var port = int.Parse(config["Plc:Port"]);

    return new ModbusTcpProtocol(ip, port);
});


builder.Services.AddSingleton<PlcMemoryCache>();
builder.Services.AddHostedService<CalibrationCacheLoader>();

builder.Services.AddScoped<ICalibrationRepository, CalibrationRepository>();
builder.Services.AddHostedService<PlcPollingBackgroundService>();

builder.Services.AddSingleton<CalibrationCurve>(sp =>
{
    var curve = new CalibrationCurve();
    curve.Fit(
        new double[] { 0, 1000, 2000 },
        new double[] { 0, 50, 100 }
    );
    return curve;
});
// ===============================
// Load PLC Tags from JSON file
// ===============================
var plcTagsFile = builder.Configuration["Plc:TagsFile"];

if (string.IsNullOrWhiteSpace(plcTagsFile))
    throw new Exception("Plc:TagsFile is not configured in appsettings.json");

// Build absolute path
var plcTagsFullPath = Path.Combine(
    builder.Environment.ContentRootPath,
    plcTagsFile
);

if (!File.Exists(plcTagsFullPath))
    throw new FileNotFoundException(
        $"PLC tags file not found: {plcTagsFullPath}"
    );

// 🔴 THIS WAS MISSING
var plcJson = File.ReadAllText(plcTagsFullPath);

// Deserialize with enum support
var plcTagConfig = JsonSerializer.Deserialize<PlcTagConfig>(
    plcJson,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    }
);

if (plcTagConfig == null || plcTagConfig.Tags.Count == 0)
    throw new Exception("Failed to load PLC tags from plc-tags.json");

// Register into DI
builder.Services.AddSingleton(plcTagConfig);





builder.Services.AddSingleton<ForceService>();
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
app.MapHub<ServoHub>("/servoHub");


// -----------------------------------------------------------------------------
// ROTATIVA (PDF)
// -----------------------------------------------------------------------------
RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa"
);

app.Run();

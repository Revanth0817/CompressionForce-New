using CompressionForce.Data;
using CompressionForce.Data.Repositories;
using CompressionForce.Data.UnitOfWork;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Abstractions.UnitOfWork;
using CompressionForce.Domain.Plc;
using CompressionForce.Domain.Validation;
using CompressionForce.Integrations.Plc;
using CompressionForce.Integrations.Plc.Batching;
using CompressionForce.Integrations.Plc.Config;
using CompressionForce.Integrations.Plc.Polling;
using CompressionForce.Services;
using CompressionForce.Services.Audit;
using CompressionForce.Services.Batches;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Plc;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.Hubs;
using CompressionForce.Web.ModelBinding;
using CompressionForce.Web.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rotativa.AspNetCore;


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

// -------------------- TRANSACTION - UNIT OF WORK --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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
builder.Services.AddScoped<IAutoTareService, AutoTareService>();
// Recipe
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
// Batch
builder.Services.AddScoped<IBatchQueryService, BatchQueryService>();
builder.Services.AddScoped<IBatchApplicationService, BatchApplicationService>();

builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<ICurrentBatchRepository, CurrentBatchRepository>();

builder.Services.AddScoped<IBatchHistoryRepository, BatchHistoryRepository>();

// -------------------- PLC & SIGNAL-R --------------------
// PLC connection
builder.Services.Configure<PlcConnectionOptions>(
    builder.Configuration.GetSection("Plc")
);

builder.Services.AddSingleton<ModbusPlcClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<PlcConnectionOptions>>().Value;
    return new ModbusPlcClient(options);
});

builder.Services.AddSingleton<IPlcClient>(sp =>
    sp.GetRequiredService<ModbusPlcClient>());

// PLC signal config
builder.Services.AddSingleton<PlcSignalRegistry>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.ContentRootPath, "plc-signals.json");
    var signals = PlcConfigLoader.Load(path);
    return new PlcSignalRegistry(signals);
});

builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<PlcSignalRegistry>();
    return PlcPollPlan.Create(registry.GetAll());
});

// PLC runtime
builder.Services.AddSingleton<PlcSignalCache>();
builder.Services.AddSingleton<IPlcBatchReader, PlcBatchReader>();
builder.Services.AddSingleton<IPlcSignalReader, ModbusPlcSignalReader>();
builder.Services.AddSingleton<IPlcSignalWriter, ModbusPlcSignalWriter>();
builder.Services.AddSingleton<IPlcWriteConfirmService, PlcWriteConfirmService>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPlcRealtimeNotifier, SignalRPlcNotifier>();

// Background polling
builder.Services.AddHostedService<PlcPollingService>();
/*
builder.Services.Configure<PlcConnectionOptions>(
    builder.Configuration.GetSection("Plc")
);

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<PlcConnectionOptions>>().Value;
    return new ModbusPlcClient(options);
});
builder.Services.AddSingleton<IPlcClient>(sp =>
    sp.GetRequiredService<ModbusPlcClient>());

builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.ContentRootPath, "plc-signals.json");
    var signals = PlcConfigLoader.Load(path);
    return new PlcSignalRegistry(signals);
});

builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<PlcSignalRegistry>();
    return PlcPollPlan.Create(registry.GetAll());
});

builder.Services.AddSingleton<PlcSignalCache>();

builder.Services.AddSingleton<IPlcBatchReader, PlcBatchReader>();

builder.Services.AddSingleton<IPlcSignalReader, ModbusPlcSignalReader>();
builder.Services.AddSingleton<IPlcSignalWriter, ModbusPlcSignalWriter>();
builder.Services.AddSingleton<IPlcWriteConfirmService, PlcWriteConfirmService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IPlcRealtimeNotifier, SignalRPlcNotifier>();

builder.Services.AddHostedService<PlcPollingService>();
*/

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
            context.Session.SetString(
                "LastActivity",
                DateTime.UtcNow.ToString("O")
            );
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

// -----------------------------------------------------------------------------
// PLC
// -----------------------------------------------------------------------------
app.MapHub<PlcHub>("/plcHub");
// -----------------------------------------------------------------------------
// ROTATIVA (PDF)
// -----------------------------------------------------------------------------
RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa"
);

app.Run();

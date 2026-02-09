using CompressionForce.Data;
using CompressionForce.Data.Repositories;
using CompressionForce.Data.UnitOfWork;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Abstractions.UnitOfWork;
using CompressionForce.Domain.Validation;
using CompressionForce.Integrations.AccessStrategies.Polling;
using CompressionForce.Integrations.Cache;
using CompressionForce.Integrations.Configuration;
using CompressionForce.Integrations.Events;
using CompressionForce.Integrations.Protocols.Modbus;
using CompressionForce.Integrations.Quality;
using CompressionForce.Integrations.Registry;
using CompressionForce.Services;
using CompressionForce.Services.Audit;
using CompressionForce.Services.Batches;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Lookups;
using CompressionForce.Services.Recipes;
using CompressionForce.Services.Validation;
using CompressionForce.Web.Hubs;
using CompressionForce.Web.ModelBinding;
using CompressionForce.Web.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System;
using System.IO;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

#region ------------------------------------------------------------------------
// CONFIGURATION FILES
// -----------------------------------------------------------------------------

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    // 🔹 Infrastructure configs (from Configurations folder)
    .AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "Configurations", "plc-connection.json"),
        optional: false,
        reloadOnChange: true)

    .AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "Configurations", "polling.json"),
        optional: false,
        reloadOnChange: true)

    .AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "Configurations", "signal-quality.json"),
        optional: false,
        reloadOnChange: true)
    // 🔹 Overrides
    .AddEnvironmentVariables();

// PLC Signals
var signalPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "Configurations",
    "signals.json");

var loader = new SignalJsonLoader();
var signals = loader.Load(signalPath);

// Recipe validation rules
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "recipe-validation.json"),
    optional: false,
    reloadOnChange: true);

#endregion

#region ------------------------------------------------------------------------
// CORE INFRASTRUCTURE SERVICES
// -----------------------------------------------------------------------------

builder.Services.AddHttpContextAccessor();

// PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region ------------------------------------------------------------------------
// UNIT OF WORK / DATA ACCESS
// -----------------------------------------------------------------------------

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

#endregion

#region ------------------------------------------------------------------------
// VALIDATION
// -----------------------------------------------------------------------------

builder.Services.Configure<RecipeValidationConfig>(builder.Configuration);
builder.Services.AddSingleton<IRecipeValidationConfigProvider, JsonRecipeValidationConfigProvider>();

builder.Services.AddScoped<ConfigRecipeValidator>();
builder.Services.AddScoped<IRecipeValidator, RecipeRulesValidator>();
builder.Services.AddScoped<LookupRecipeValidator>();

#endregion

#region ------------------------------------------------------------------------
// APPLICATION SERVICES
// -----------------------------------------------------------------------------

builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IPlcStatusService, PlcStatusService>();
builder.Services.AddScoped<AutoTareService>();

#endregion

#region ------------------------------------------------------------------------
// REPOSITORIES
// -----------------------------------------------------------------------------

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

builder.Services.AddScoped<IBatchQueryService, BatchQueryService>();
builder.Services.AddScoped<IBatchApplicationService, BatchApplicationService>();

builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<ICurrentBatchRepository, CurrentBatchRepository>();
builder.Services.AddScoped<IBatchHistoryRepository, BatchHistoryRepository>();

#endregion

#region ------------------------------------------------------------------------
// AUDIT
// -----------------------------------------------------------------------------

builder.Services.AddScoped<AuditLogger>();

#endregion

#region ------------------------------------------------------------------------
// SIGNALR
// -----------------------------------------------------------------------------
// Live signal streaming to UI
builder.Services.AddSignalR();
#endregion


#region ------------------------------------------------------------------------
// MVC + MODEL BINDING
// -----------------------------------------------------------------------------

builder.Services
    .AddControllersWithViews(options =>
    {
        options.ModelBinderProviders.Insert(
            0,
            new RecipeParameterModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .AddSessionStateTempDataProvider();

#endregion

#region ------------------------------------------------------------------------
// PLC SIGNAL REGISTRY + CACHE
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<IPlcSignalRegistry>(
    new PlcSignalRegistry(signals));

builder.Services.AddSingleton<IPlcSignalCache, PlcSignalCache>();

#endregion

#region ------------------------------------------------------------------------
// POLLING + QUALITY + EVENT PIPELINE
// -----------------------------------------------------------------------------

builder.Services.AddSingleton<IPlcClient, ModbusPlcClient>();


builder.Services.Configure<PollingConfig>(
    builder.Configuration.GetSection("polling"));

builder.Services.Configure<SignalQualityConfig>(
    builder.Configuration.GetSection("signal-quality"));


// Polling access strategy
builder.Services.AddSingleton<PollingAccessStrategy>();

// Quality evaluation (used by event pump)
builder.Services.AddSingleton<SignalQualityEvaluator>();

// Signal event pump (publishes from cache)
builder.Services.AddSingleton<ISignalEventPump, SignalEventPump>();

// Polling intervals (from polling.json)
builder.Services.AddSingleton<IPollingIntervalProvider,
    JsonPollingIntervalProvider>();

// Staleness policy (from signal-quality.json)
builder.Services.AddSingleton<ISignalStalenessPolicy,
    JsonSignalStalenessPolicy>();

// Event pump intervals (UpdateClass-based)
builder.Services.AddSingleton<IEventPumpIntervalProvider,
    JsonEventPumpIntervalProvider>();

// Web owns SignalR, implements Domain abstraction
builder.Services.AddSingleton<ISignalEventSink, SignalRSignalEventSink>();
#endregion


#region ------------------------------------------------------------------------
// SESSION
// -----------------------------------------------------------------------------

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CompressionForce.Session";
});

#endregion

var app = builder.Build();

#region ------------------------------------------------------------------------
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

#region ------------------------------------------------------------------------
// SIGNALR HUBS
// -----------------------------------------------------------------------------

app.MapHub<SignalHub>("/hubs/signals");
// (Later)
// app.MapHub<AlarmHub>("/hubs/alarms");

#endregion


// ⚠️ Session MUST be before custom middleware
app.UseSession();

// -----------------------------------------------------------------------------
// APPLICATION TIMEOUT MIDDLEWARE (UNCHANGED)
// -----------------------------------------------------------------------------

app.Use(async (context, next) =>
{
    var username = context.Session.GetString("UserName");

    if (!string.IsNullOrEmpty(username))
    {
        var db =
            context.RequestServices.GetRequiredService<ApplicationDbContext>();

        var settings =
            await db.SecuritySettings.FirstOrDefaultAsync();

        if (settings != null &&
            settings.ApplicationTimeoutMinutes > 0)
        {
            var lastActivityStr =
                context.Session.GetString("LastActivity");

            if (!string.IsNullOrEmpty(lastActivityStr) &&
                DateTime.TryParse(lastActivityStr, out var lastActivity))
            {
                var idleMinutes =
                    (DateTime.UtcNow - lastActivity).TotalMinutes;

                if (idleMinutes >
                    settings.ApplicationTimeoutMinutes)
                {
                    context.Session.Clear();
                    return;
                }
            }

            context.Session.SetString(
                "LastActivity",
                DateTime.UtcNow.ToString("O"));
        }
    }

    await next();
});

// app.UseAuthentication();
app.UseAuthorization();

#endregion

#region ------------------------------------------------------------------------
// ROUTES
// -----------------------------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

#endregion

#region ------------------------------------------------------------------------
// SIGNAL PIPELINE STARTUP (POLLING + EVENT PUMP)
// -----------------------------------------------------------------------------

var registry =
    app.Services.GetRequiredService<IPlcSignalRegistry>();

var allSignals = registry.GetAllPrimaries();


// Start PLC polling
var polling =
    app.Services.GetRequiredService<PollingAccessStrategy>();

polling.Start(allSignals);

// Start signal event pump
var eventPump =
    app.Services.GetRequiredService<ISignalEventPump>();

var lifetime =
    app.Services.GetRequiredService<IHostApplicationLifetime>();

_ = eventPump.StartAsync(
    allSignals,
    lifetime.ApplicationStopping);

#endregion

#region ------------------------------------------------------------------------
// ROTATIVA (PDF)
// -----------------------------------------------------------------------------

RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa");

#endregion

app.Run();

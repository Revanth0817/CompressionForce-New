using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CompressionForce.Domain.Calibration;

namespace CompressionForce.Services;

public class CalibrationCacheLoader : IHostedService
{
    private readonly PlcMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;

    public CalibrationCacheLoader(
        PlcMemoryCache cache,
        IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 🔑 CREATE SCOPE
        using var scope = _scopeFactory.CreateScope();

        var repo = scope.ServiceProvider
            .GetRequiredService<ICalibrationRepository>();

        var calibrations = await repo.GetAllAsync();

        foreach (var c in calibrations)
        {
            _cache.Set($"{c.LoadCellCode}_FACTOR", c.Factor);
            _cache.Set($"{c.LoadCellCode}_OFFSET", c.Offset);

            Console.WriteLine(
                $"✅ CAL LOADED {c.LoadCellCode} F={c.Factor} O={c.Offset}"
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

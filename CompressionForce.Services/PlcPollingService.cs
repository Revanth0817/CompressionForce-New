using Microsoft.Extensions.Hosting;
using CompressionForce.Domain.ViewModels;

namespace CompressionForce.Services;

public class PlcPollingService : BackgroundService
{
    private readonly DiagnosticsService _service;
    private readonly DiagnosticsStateStore _store;

    public PlcPollingService(
        DiagnosticsService service,
        DiagnosticsStateStore store)
    {
        _service = service;
        _store = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DiagnosticsFixedVM data = _service.Read();
                _store.Update(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLC Poll error: " + ex.Message);
            }

            await Task.Delay(200, stoppingToken);

        }
    }
}

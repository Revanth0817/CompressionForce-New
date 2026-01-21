using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using CompressionForce.Services;
using CompressionForce.Web.Hubs;

namespace CompressionForce.Web.Services;

public class DiagnosticsPushService : BackgroundService
{
    private readonly DiagnosticsStateStore _store;
    private readonly IHubContext<DiagnosticsHub> _hub;

    public DiagnosticsPushService(
        DiagnosticsStateStore store,
        IHubContext<DiagnosticsHub> hub)
    {
        _store = store;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_store.Current != null)
            {
                await _hub.Clients.All.SendAsync(
                    "DiagnosticsUpdate",
                    _store.Current,
                    cancellationToken: stoppingToken);
            }

            await Task.Delay(500, stoppingToken);
        }
    }
}

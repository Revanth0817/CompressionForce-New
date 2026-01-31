using CompressionForce.Services.Interfaces;
using CompressionForce.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CompressionForce.Web.SignalR
{
    public sealed class SignalRPlcNotifier : IPlcRealtimeNotifier
    {
        private readonly IHubContext<PlcHub> _hub;

        public SignalRPlcNotifier(IHubContext<PlcHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyAsync(string key, object value)
            => _hub.Clients.All.SendAsync("plcUpdate", key, value);
    }
}

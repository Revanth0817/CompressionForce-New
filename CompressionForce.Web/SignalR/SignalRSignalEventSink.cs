using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Events;
using CompressionForce.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CompressionForce.Web.SignalR
{
    // ✅ Web owns SignalR
    // ✅ Implements Domain abstraction
    public sealed class SignalRSignalEventSink : ISignalEventSink
    {
        private readonly IHubContext<SignalHub> _hub;

        public SignalRSignalEventSink(
            IHubContext<SignalHub> hub)
        {
            _hub = hub;
        }

        public Task PublishAsync(SignalEvent evt)
        {
            return _hub.Clients.All.SendAsync(
                "signalUpdated",
                new
                {
                    id = evt.SignalId,
                    value = evt.Value,
                    quality = evt.Quality.ToString(),
                    timestampUtc = evt.TimestampUtc
                });
        }
    }
}

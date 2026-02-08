using System.Collections.Generic;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Events;

namespace CompressionForce.Integrations.Events
{
    public sealed class SignalEventBus : ISignalEventSink
    {
        private readonly IReadOnlyList<ISignalEventSink> _sinks;

        public SignalEventBus(IEnumerable<ISignalEventSink> sinks)
        {
            _sinks = new List<ISignalEventSink>(sinks);
        }

        public async Task PublishAsync(SignalEvent signalEvent)
        {
            foreach (var sink in _sinks)
            {
                await sink.PublishAsync(signalEvent);
            }
        }
    }
}

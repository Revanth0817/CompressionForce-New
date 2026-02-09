using System;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Events;
using CompressionForce.Integrations.Quality;

namespace CompressionForce.Integrations.Events
{
    public sealed class SignalEventPublisher
    {
        private readonly IPlcSignalRegistry _registry;
        private readonly IPlcSignalCache _cache;
        private readonly SignalQualityEvaluator _qualityEvaluator;
        private readonly ISignalEventSink _sink;

        public SignalEventPublisher(
            IPlcSignalRegistry registry,
            IPlcSignalCache cache,
            SignalQualityEvaluator qualityEvaluator,
            ISignalEventSink sink)
        {
            _registry = registry;
            _cache = cache;
            _qualityEvaluator = qualityEvaluator;
            _sink = sink;
        }

        public async Task PublishAsync(string signalId)
        {
            var signal = _registry.Resolve(signalId);
            var value = _cache.Get(signalId);

            var quality = _qualityEvaluator.Evaluate(signal, value);

            var evt = new SignalEvent(
                signalId,
                value?.Value,
                quality,
                value?.TimestampUtc ?? DateTime.UtcNow);

            Console.WriteLine( $"[SignalEventPump] {signal.SignalId} = {value?.Value} ({quality}) : 1");

            await _sink.PublishAsync(evt);
        }
    }
}

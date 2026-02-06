using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;


namespace CompressionForce.Integrations.AccessStrategies.Polling
{
    internal sealed class PollingWorker
    {
        private readonly int _intervalMs;
        private readonly IEnumerable<PlcSignal> _signals;
        private readonly IPlcClient _plcClient;
        private readonly IPlcSignalCache _cache;
        private readonly ISignalQualityEvaluator _qualityEvaluator;

        public PollingWorker(
            int intervalMs,
            IEnumerable<PlcSignal> signals,
            IPlcClient plcClient,
            IPlcSignalCache cache,
            ISignalQualityEvaluator qualityEvaluator)
        {
            _intervalMs = intervalMs;
            _signals = signals;
            _plcClient = plcClient;
            _cache = cache;
            _qualityEvaluator = qualityEvaluator;
        }

        public async Task RunAsync(CancellationToken token)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromMilliseconds(_intervalMs));

            while (await timer.WaitForNextTickAsync(token))
            {
                IDictionary<string, object> values = null;

                try
                {
                    values = await _plcClient.ReadAsync(_signals);
                }
                catch
                {
                    values = null;
                }

                foreach (var signal in _signals)
                {
                    var previous = _cache.Get(signal.SignalId);
                    var now = DateTime.UtcNow;

                    SignalValue current = values != null && values.TryGetValue(signal.SignalId, out var val)
                        ? new SignalValue
                        {
                            Value = val,
                            TimestampUtc = now,
                            Quality = SignalQuality.Good
                        }
                        : null;

                    var evaluated = _qualityEvaluator.Evaluate(
                        previous,
                        current,
                        _intervalMs);

                    _cache.Set(signal.SignalId, evaluated);
                }
            }
        }
    }
}

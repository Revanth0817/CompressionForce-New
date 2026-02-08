using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.AccessStrategies.Polling
{
    internal sealed class PollingWorker
    {
        private readonly int _intervalMs;
        private readonly IReadOnlyList<PlcSignal> _signals;
        private readonly IPlcClient _plcClient;
        private readonly IPlcSignalCache _cache;

        public PollingWorker(
            int intervalMs,
            IReadOnlyList<PlcSignal> signals,
            IPlcClient plcClient,
            IPlcSignalCache cache)
        {
            _intervalMs = intervalMs;
            _signals = signals;
            _plcClient = plcClient;
            _cache = cache;
        }

        public async Task RunAsync(CancellationToken token)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromMilliseconds(_intervalMs));

            while (await timer.WaitForNextTickAsync(token))
            {
                IDictionary<string, object>? values = null;

                try
                {
                    values = await _plcClient.ReadAsync(_signals);
                }
                catch
                {
                    // PLC read failure → skip this cycle
                    continue;
                }

                var now = DateTime.UtcNow;

                foreach (var signal in _signals)
                {
                    if (values != null &&
                        values.TryGetValue(signal.SignalId, out var val))
                    {
                        _cache.Set(
                            signal.SignalId,
                            new SignalValue(
                                val,
                                now,
                                Domain.Enums.SignalQuality.Good
                            ));
                    }
                    else
                    {
                        _cache.Set(
                            signal.SignalId,
                            SignalValue.Unknown());
                    }
                }
            }
        }
    }
}

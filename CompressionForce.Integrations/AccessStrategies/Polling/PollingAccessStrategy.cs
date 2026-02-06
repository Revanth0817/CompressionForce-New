using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Integrations.Abstractions;

namespace CompressionForce.Integrations.AccessStrategies.Polling
{
    public sealed class PollingAccessStrategy : IPlcAccessStrategy
    {
        private readonly IPlcClient _client;
        private readonly IPlcSignalCache _cache;
        private readonly Dictionary<UpdateClass, int> _intervals;
        private readonly List<CancellationTokenSource> _tokens = new();

        public PollingAccessStrategy(
            IPlcClient client,
            IPlcSignalCache cache,
            Dictionary<UpdateClass, int> intervals)
        {
            _client = client;
            _cache = cache;
            _intervals = intervals;
        }

        public void Start(IEnumerable<PlcSignal> signals)
        {
            foreach (var group in signals.GroupBy(s => s.UpdateClass))
            {
                var cts = new CancellationTokenSource();
                _tokens.Add(cts);
                _ = RunPollingLoop(group.Key, group.ToList(), cts.Token);
            }
        }

        private async Task RunPollingLoop(
            UpdateClass updateClass,
            List<PlcSignal> signals,
            CancellationToken token)
        {
            var delay = _intervals[updateClass];

            while (!token.IsCancellationRequested)
            {
                var values = await _client.ReadAsync(signals);

                foreach (var kv in values)
                {
                    _cache.Set(kv.Key, new SignalValue
                    {
                        Value = kv.Value,
                        Timestamp = DateTime.UtcNow,
                        IsGood = true
                    });
                }

                await Task.Delay(delay, token);
            }
        }

        public void Stop()
        {
            foreach (var t in _tokens)
                t.Cancel();
        }
    }
}

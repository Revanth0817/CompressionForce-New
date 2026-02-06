using System.Collections.Concurrent;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Cache
{
    public sealed class PlcSignalCache : IPlcSignalCache
    {
        private readonly ConcurrentDictionary<string, SignalValue> _cache = new();

        public SignalValue Get(string signalId)
        {
            if (!_cache.TryGetValue(signalId, out var value))
                throw new InvalidOperationException(
                    $"No cached value available for signal '{signalId}'");

            return value;
        }


        public void Set(string signalId, SignalValue value)
            => _cache[signalId] = value;
    }
}

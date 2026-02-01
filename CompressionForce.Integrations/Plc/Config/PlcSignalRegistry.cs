using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Plc;


namespace CompressionForce.Integrations.Plc.Config
{
    public sealed class PlcSignalRegistry
    {
        private readonly Dictionary<string, PlcSignalDefinition> _signals;

        public PlcSignalRegistry(IEnumerable<PlcSignalDefinition> signals)
        {
            if (signals == null)
                throw new ArgumentNullException(nameof(signals));

            var list = signals.ToList();

            var invalid = list
                .Where(s => string.IsNullOrWhiteSpace(s.Key))
                .ToList();

            if (invalid.Any())
            {
                throw new InvalidOperationException(
                    $"PLC config error: {invalid.Count} signal(s) have null/empty keys");
            }

            var duplicates = list
                .GroupBy(s => s.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException(
                    $"PLC config error: duplicate signal keys: {string.Join(", ", duplicates)}");
            }

            _signals = list.ToDictionary(s => s.Key);
        }


        // ✅ FIX FOR YOUR ERROR
        public IReadOnlyCollection<PlcSignalDefinition> GetAll()
            => _signals.Values;

        public PlcSignalDefinition Get(string key)
            => _signals.TryGetValue(key, out var s)
                ? s
                : throw new KeyNotFoundException($"PLC signal '{key}' not found");

        public bool TryGet(string key, out PlcSignalDefinition signal)
            => _signals.TryGetValue(key, out signal);
    }
}


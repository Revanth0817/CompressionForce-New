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
            _signals = signals.ToDictionary(s => s.Key);
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


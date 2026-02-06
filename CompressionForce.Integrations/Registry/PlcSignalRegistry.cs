using System.Collections.Generic;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Registry
{
    public sealed class PlcSignalRegistry : IPlcSignalRegistry
    {
        public IReadOnlyDictionary<string, PlcSignal> Signals { get; }

        public PlcSignalRegistry(IReadOnlyDictionary<string, PlcSignal> signals)
        {
            Signals = signals;
        }

        public PlcSignal Get(string signalId)
        {
            return Signals.TryGetValue(signalId, out var s) ? s : null;
        }
    }
}

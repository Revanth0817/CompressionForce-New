using System.Collections.Generic;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IPlcSignalRegistry
    {
        IReadOnlyDictionary<string, PlcSignal> Signals { get; }
        PlcSignal Get(string signalId);
    }
}

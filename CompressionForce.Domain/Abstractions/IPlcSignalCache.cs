using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IPlcSignalCache
    {
        SignalValue Get(string signalId);
        void Set(string signalId, SignalValue value);
    }
}

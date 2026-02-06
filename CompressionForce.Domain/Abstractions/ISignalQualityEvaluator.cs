using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface ISignalQualityEvaluator
    {
        SignalValue Evaluate(
            SignalValue previous,
            SignalValue current,
            int expectedUpdateMs);
    }
}

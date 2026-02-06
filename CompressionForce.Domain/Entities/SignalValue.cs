using System;

namespace CompressionForce.Domain.Entities
{
    public sealed class SignalValue
    {
        public object Value { get; init; }
        public DateTime Timestamp { get; init; }
        public bool IsGood { get; init; }
    }
}
using System;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Entities
{
    public sealed class SignalValue
    {
        public object Value { get; init; }
        public DateTime TimestampUtc { get; init; }
        public SignalQuality Quality { get; init; }
    }
}

using System;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Entities
{
    public sealed class SignalValue
    {
        public object Value { get; }
        public DateTime TimestampUtc { get; }
        public SignalQuality Quality { get; }

        public SignalValue(
            object value,
            DateTime timestampUtc,
            SignalQuality quality)
        {
            Value = value;
            TimestampUtc = timestampUtc;
            Quality = quality;
        }

        public static SignalValue Unknown()
            => new(null, DateTime.MinValue, SignalQuality.Unknown);

        public SignalValue WithQuality(SignalQuality quality)
            => new(Value, TimestampUtc, quality);
    }
}

using System;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Events
{
    public sealed class SignalEvent
    {
        public string SignalId { get; }
        public object Value { get; }
        public SignalQuality Quality { get; }
        public DateTime TimestampUtc { get; }

        public SignalEvent(
            string signalId,
            object value,
            SignalQuality quality,
            DateTime timestampUtc)
        {
            SignalId = signalId;
            Value = value;
            Quality = quality;
            TimestampUtc = timestampUtc;
        }
    }
}

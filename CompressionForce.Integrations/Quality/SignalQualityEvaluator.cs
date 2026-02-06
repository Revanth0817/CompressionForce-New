using System;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Integrations.Quality
{
    public sealed class SignalQualityEvaluator : ISignalQualityEvaluator
    {
        private const double StaleMultiplier = 2.0;
        private const double CommLostMultiplier = 5.0;

        public SignalValue Evaluate(
            SignalValue previous,
            SignalValue current,
            int expectedUpdateMs)
        {
            var now = DateTime.UtcNow;

            // No new value received
            if (current == null)
            {
                return new SignalValue
                {
                    Value = previous?.Value,
                    TimestampUtc = previous?.TimestampUtc ?? now,
                    Quality = SignalQuality.CommunicationLost
                };
            }

            var ageMs = (now - current.TimestampUtc).TotalMilliseconds;

            if (ageMs <= expectedUpdateMs * StaleMultiplier)
            {
                return new SignalValue
                {
                    Value = current.Value,
                    TimestampUtc = current.TimestampUtc,
                    Quality = SignalQuality.Good
                };
            }

            if (ageMs <= expectedUpdateMs * CommLostMultiplier)
            {
                return new SignalValue
                {
                    Value = current.Value,
                    TimestampUtc = current.TimestampUtc,
                    Quality = SignalQuality.Stale
                };
            }

            return new SignalValue
            {
                Value = current.Value,
                TimestampUtc = current.TimestampUtc,
                Quality = SignalQuality.CommunicationLost
            };
        }
    }
}

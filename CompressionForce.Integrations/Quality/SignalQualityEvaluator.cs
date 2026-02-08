using System;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Integrations.Quality
{
    public sealed class SignalQualityEvaluator
    {
        private readonly ISignalStalenessPolicy _policy;

        public SignalQualityEvaluator(ISignalStalenessPolicy policy)
        {
            _policy = policy;
        }

        public SignalQuality Evaluate(
            PlcSignal signal,
            SignalValue value)
        {
            if (value == null)
                return SignalQuality.Unknown;

            if (value.Quality == SignalQuality.Bad)
                return SignalQuality.Bad;

            var age = DateTime.UtcNow - value.TimestampUtc;
            var maxAge = _policy.GetMaxAge(signal.UpdateClass);

            return age <= maxAge
                ? SignalQuality.Good
                : SignalQuality.Stale;
        }
    }
}

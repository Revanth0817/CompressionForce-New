using System.Collections.Generic;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;

namespace CompressionForce.Integrations.Polling
{
    public sealed class PollingIntervalProvider : IPollingIntervalProvider
    {
        private readonly bool _useGlobal;
        private readonly int _globalMs;
        private readonly IReadOnlyDictionary<UpdateClass, int> _intervals;

        public PollingIntervalProvider(
            bool useGlobalFrequency,
            int globalFrequencyMs,
            IReadOnlyDictionary<UpdateClass, int> intervals)
        {
            _useGlobal = useGlobalFrequency;
            _globalMs = globalFrequencyMs;
            _intervals = intervals;
        }

        public int GetIntervalMilliseconds(UpdateClass updateClass)
        {
            if (_useGlobal)
                return _globalMs;

            return _intervals[updateClass];
        }
    }
}

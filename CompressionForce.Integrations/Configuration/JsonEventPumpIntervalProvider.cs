using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class JsonEventPumpIntervalProvider
        : IEventPumpIntervalProvider
    {
        private readonly PollingConfig _config;

        public JsonEventPumpIntervalProvider(
            IOptions<PollingConfig> options)
        {
            _config = options.Value;
        }

        public int GetIntervalMilliseconds(UpdateClass updateClass)
        {
            var key = updateClass.ToString();

            // 1️⃣ Explicit override wins
            if (_config.EventPump.Overrides.TryGetValue(key, out var overrideMs))
                return overrideMs;

            // 2️⃣ Use polling frequency if configured
            if (_config.EventPump.UsePollingFrequency &&
                _config.Frequencies.TryGetValue(key, out var pollingMs))
                return pollingMs;

            // 3️⃣ Fallback
            return _config.GlobalFrequencyMs;
        }
    }
}

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
            //Console.WriteLine( $"[INTERVAL] {updateClass} => {_config.GlobalFrequencyMs} ms");

            var key = updateClass.ToString();

            // 1️⃣ Explicit override wins
            if (_config.EventPump?.Overrides != null &&
                _config.EventPump.Overrides.TryGetValue(key, out var overrideMs) &&
                overrideMs > 0)
            {
                return overrideMs;
            }

            // 2️⃣ Follow polling frequency
            if (_config.EventPump?.UsePollingFrequency == true &&
                _config.Frequencies != null &&
                _config.Frequencies.TryGetValue(key, out var pollingMs) &&
                pollingMs > 0)
            {
                return pollingMs;
            }

            // 3️⃣ Global fallback
            if (_config.GlobalFrequencyMs > 0)
                return _config.GlobalFrequencyMs;

            // 4️⃣ Absolute safety
            return 1000;
        }
    }
}

using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class JsonPollingIntervalProvider : IPollingIntervalProvider
    {
        private readonly PollingConfig _config;

        public JsonPollingIntervalProvider(IOptions<PollingConfig> options)
        {
            _config = options.Value;
        }

        public int GetIntervalMilliseconds(UpdateClass updateClass)
        {
            //Console.WriteLine($"[INTERVAL] {updateClass} => {_config.GlobalFrequencyMs} ms");
            var key = updateClass.ToString();

            if (_config.UseGlobalFrequency &&
                _config.GlobalFrequencyMs > 0)
            {
                return _config.GlobalFrequencyMs;
            }

            if (_config.Frequencies != null &&
                _config.Frequencies.TryGetValue(key, out var ms) &&
                ms > 0)
            {
                return ms;
            }

            // 🔒 Absolute safety fallback
            return 1000;
        }

    }

}




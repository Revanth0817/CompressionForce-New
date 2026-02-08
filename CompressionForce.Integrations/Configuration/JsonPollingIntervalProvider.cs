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
            if (_config.UseGlobalFrequency)
                return _config.GlobalFrequencyMs;

            return _config.Frequencies.TryGetValue(updateClass.ToString(), out var ms)
                ? ms
                : _config.GlobalFrequencyMs;
        }
    }

}




using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Enums;
using CompressionForce.Integrations.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class JsonSignalStalenessPolicy : ISignalStalenessPolicy
    {
        private readonly SignalQualityConfig _config;

        public JsonSignalStalenessPolicy(IOptions<SignalQualityConfig> options)
        {
            _config = options.Value;
        }

        public TimeSpan GetMaxAge(UpdateClass updateClass)
        {
            return _config.StalenessPolicy.TryGetValue(updateClass.ToString(), out var ts)
                ? ts
                : TimeSpan.MaxValue;
        }
    }

}




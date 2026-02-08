using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class SignalQualityConfig
    {
        public Dictionary<string, TimeSpan> StalenessPolicy { get; set; } = new();
    }

}

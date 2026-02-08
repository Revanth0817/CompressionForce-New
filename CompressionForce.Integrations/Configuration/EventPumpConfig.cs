using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class EventPumpConfig
    {
        public bool UsePollingFrequency { get; set; }
        public Dictionary<string, int> Overrides { get; set; } = new();
    }
}

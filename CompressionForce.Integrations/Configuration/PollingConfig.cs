using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Configuration
{
    public sealed class PollingConfig
    {
        public bool UseGlobalFrequency { get; set; }
        public int GlobalFrequencyMs { get; set; }
        public Dictionary<string, int> Frequencies { get; set; } = new();
        public EventPumpConfig EventPump { get; set; } = new();
    }

}

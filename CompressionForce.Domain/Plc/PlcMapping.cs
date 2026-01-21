using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Plc
{
    public class PlcMapping
    {
        public PlcConfig Plc { get; set; }

        public Dictionary<string, int> DigitalInputs { get; set; }
        public Dictionary<string, int> DigitalOutputs { get; set; }
        public Dictionary<string, int> AnalogInputs { get; set; }
        public Dictionary<string, int> Counters { get; set; }
        public Dictionary<string, ServoMap> Servos { get; set; }
    }

    public class PlcConfig
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }

}

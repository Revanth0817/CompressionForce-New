using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain
{
    public class PlcRoot
    {
        public PlcConfig Plc { get; set; }
        public List<DigitalPoint> DigitalInputs { get; set; }
        public List<DigitalPoint> DigitalOutputs { get; set; }
        public List<AnalogPoint> AnalogInputs { get; set; }
        public List<CounterPoint> Counters { get; set; }
    }

    public class PlcConfig
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }

    public class DigitalPoint
    {
        public string Name { get; set; }
        public int Address { get; set; }
        public bool Value { get; set; }
    }

    public class AnalogPoint
    {
        public string Name { get; set; }
        public int Address { get; set; }
        public double Kn { get; set; }
        public double Volt { get; set; }
    }

    public class CounterPoint
    {
        public string Name { get; set; }
        public int Address { get; set; }
        public long Value { get; set; }
    }

}

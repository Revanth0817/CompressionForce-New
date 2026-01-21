using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CompressionForce.Domain.PLC
{
    public class PlcTagConfig
    {
        public Dictionary<string, int> PollingIntervals { get; set; }
            = new();

        public List<PlcTag> Tags { get; set; }
            = new();
    }
}

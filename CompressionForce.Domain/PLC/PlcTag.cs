using CompressionForce.Domain.PLC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.PLC
{
    public class PlcTag
    {
        public string Key { get; set; } = string.Empty;

        public string? LoadCellCode { get; set; }

        public PlcDataType Type { get; set; }

        public int Address { get; set; }

        public string Polling { get; set; } = "Medium";

        public bool Writable { get; set; }
    }
}

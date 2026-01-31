using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Plc
{
    public sealed class PlcSignalDefinition
    {
        public string Key { get; init; } = default!;
        public PlcRegisterType Type { get; init; }
        public int Address { get; init; }
        public decimal? Scale { get; init; }
        public bool Writable { get; init; }
        public PollingClass Polling { get; init; }
    }
}

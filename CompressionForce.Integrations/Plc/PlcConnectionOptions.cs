using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc
{
    public sealed class PlcConnectionOptions
    {
        public string IpAddress { get; init; } = default!;
        public int Port { get; init; } = 502;
        public int ConnectionTimeoutMs { get; init; } = 2000;
    }
}

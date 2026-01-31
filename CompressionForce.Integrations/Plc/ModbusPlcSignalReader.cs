using CompressionForce.Integrations.Plc.Config;
using CompressionForce.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc
{
    public sealed class ModbusPlcSignalReader : IPlcSignalReader
    {
        private readonly IPlcClient _plc;
        private readonly PlcSignalRegistry _registry;

        public ModbusPlcSignalReader(IPlcClient plc, PlcSignalRegistry registry)
        {
            _plc = plc;
            _registry = registry;
        }

        public decimal ReadDecimal(string key)
        {
            var sig = _registry.Get(key);
            var raw = _plc.ReadHoldingRegisters(sig.Address, 1)[0];
            return sig.Scale.HasValue ? raw * sig.Scale.Value : raw;
        }
    }
}

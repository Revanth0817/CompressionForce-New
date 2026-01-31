using CompressionForce.Domain.Plc;
using CompressionForce.Integrations.Plc.Config;
using CompressionForce.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.Plc
{
    public sealed class ModbusPlcSignalWriter : IPlcSignalWriter
    {
        private readonly IPlcClient _plc;
        private readonly PlcSignalRegistry _registry;

        public ModbusPlcSignalWriter(IPlcClient plc, PlcSignalRegistry registry)
        {
            _plc = plc;
            _registry = registry;
        }

        public Task WriteAsync(string key, object value)
        {
            var sig = _registry.Get(key);

            if (!sig.Writable)
                throw new InvalidOperationException("Signal is read-only");

            if (sig.Type == PlcRegisterType.Coil)
                _plc.WriteSingleCoil(sig.Address, Convert.ToBoolean(value));
            else
                _plc.WriteSingleRegister(sig.Address, Convert.ToInt32(value));

            return Task.CompletedTask;
        }
    }

}

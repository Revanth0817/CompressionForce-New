using System.Collections.Generic;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Protocols.Modbus
{
    public sealed class ModbusReadRange
    {
        public ModbusRegisterType RegisterType { get; }
        public ushort StartAddress { get; }
        public ushort Length { get; }
        public IReadOnlyList<PlcSignal> Signals { get; }

        public ModbusReadRange(
            ModbusRegisterType registerType,
            ushort startAddress,
            ushort length,
            IReadOnlyList<PlcSignal> signals)
        {
            RegisterType = registerType;
            StartAddress = startAddress;
            Length = length;
            Signals = signals;
        }
    }
}

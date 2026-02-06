using System.Collections.Generic;
using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Protocols.Modbus
{
    public sealed class ModbusPlcClient : IPlcClient
    {
        public Task ConnectAsync() => Task.CompletedTask;
        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<IDictionary<string, object>> ReadAsync(IEnumerable<PlcSignal> signals)
        {
            var result = new Dictionary<string, object>();

            // ✅ Static call — correct
            var ranges = ModbusReadRangeBuilder.Build(signals);

            foreach (var range in ranges)
            {
                // 🔽 Replace with real Modbus calls
                switch (range.RegisterType)
                {
                    case ModbusRegisterType.Coil:
                        // Read Coils (FC01)
                        break;

                    case ModbusRegisterType.DiscreteInput:
                        // Read Discrete Inputs (FC02)
                        break;

                    case ModbusRegisterType.HoldingRegister:
                        // Read Holding Registers (FC03)
                        break;

                    case ModbusRegisterType.InputRegister:
                        // Read Input Registers (FC04)
                        break;
                }

                // Stub mapping: map each signal in the range
                foreach (var s in range.Signals)
                {
                    result[s.SignalId] = 0; // replace with decoded value
                }
            }

            return Task.FromResult<IDictionary<string, object>>(result);
        }

        public Task WriteAsync(string signalId, object value)
        {
            // Writes allowed only for:
            // - Coil
            // - HoldingRegister
            return Task.CompletedTask;
        }
    }
}

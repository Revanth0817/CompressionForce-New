using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Plc;
using CompressionForce.Integrations.Plc.Config;

namespace CompressionForce.Integrations.Plc.Batching
{
    public sealed class PlcBatchReader : IPlcBatchReader
    {
        private readonly IPlcClient _plc;
        private readonly PlcSignalRegistry _registry;

        public PlcBatchReader(
            IPlcClient plc,
            PlcSignalRegistry registry)
        {
            _plc = plc;
            _registry = registry;
        }


        public IDictionary<string, object> ReadBatch(PlcPollBatch batch)
        {
            var result = new Dictionary<string, object>();

            foreach (var address in batch.Addresses)
            {
                object? value = batch.RegisterType switch
                {
                    PlcRegisterType.Coil =>
                        _plc.ReadCoils(address, 1)[0],

                    PlcRegisterType.InputRegister =>
                        _plc.ReadInputRegisters(address, 1)[0],

                    PlcRegisterType.HoldingRegister =>
                        _plc.ReadHoldingRegisters(address, 1)[0],

                    _ => null
                };

                if (value == null)
                    continue;

                var signal = _registry.GetAll()
                    .FirstOrDefault(s =>
                        s.Type == batch.RegisterType &&
                        s.Address == address);

                if (signal == null)
                    continue;

                // Apply scaling if configured
                if (signal.Scale.HasValue && value is int raw)
                    result[signal.Key] = raw * signal.Scale.Value;
                else
                    result[signal.Key] = value;
            }

            return result;
        }
    }
}


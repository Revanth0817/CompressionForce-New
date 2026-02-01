using CompressionForce.Domain.Abstractions;
using CompressionForce.Domain.Plc;

namespace CompressionForce.Integrations.Plc.Batching
{
    public sealed class PlcBatchReader : IPlcBatchReader
    {
        private readonly IPlcClient _plc;

        public PlcBatchReader(IPlcClient plc)
        {
            _plc = plc;
        }

        public IDictionary<string, object> ReadBatch(PlcPollBatch batch)
        {
            var result = new Dictionary<string, object>();

            foreach (var address in batch.Addresses)
            {
                switch (batch.RegisterType)
                {
                    case PlcRegisterType.Coil:
                        result[$"COIL_{address}"] =
                            _plc.ReadCoils(address, 1)[0];
                        break;

                    case PlcRegisterType.InputRegister:
                        result[$"IR_{address}"] =
                            _plc.ReadInputRegisters(address, 1)[0];
                        break;

                    case PlcRegisterType.HoldingRegister:
                        result[$"HR_{address}"] =
                            _plc.ReadHoldingRegisters(address, 1)[0];
                        break;
                }
            }


            return result;
        }

        /*private static void Map<T>(
            PlcRegisterRange range,
            T[] values,
            string prefix,
            IDictionary<string, object> target)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var key = $"{prefix}_{range.StartAddress + i}";
                target[key] = values[i]!;
            }
        }*/
    }
}


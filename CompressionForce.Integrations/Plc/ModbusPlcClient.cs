using EasyModbus; // or whatever Modbus lib you use

namespace CompressionForce.Integrations.Plc
{
    public sealed class ModbusPlcClient : IPlcClient
    {
        private readonly ModbusClient _client;

        public ModbusPlcClient(PlcConnectionOptions options)
        {
            _client = new ModbusClient(
                options.IpAddress,
                options.Port
            );

            _client.ConnectionTimeout = options.ConnectionTimeoutMs;
            _client.Connect();
        }

        public bool[] ReadCoils(int start, int count)
            => _client.ReadCoils(start, count);

        public bool[] ReadDiscreteInputs(int start, int count)
            => _client.ReadDiscreteInputs(start, count);

        public int[] ReadInputRegisters(int start, int count)
            => _client.ReadInputRegisters(start, count);

        public int[] ReadHoldingRegisters(int start, int count)
            => _client.ReadHoldingRegisters(start, count);

        public void WriteSingleCoil(int address, bool value)
            => _client.WriteSingleCoil(address, value);

        public void WriteSingleRegister(int address, int value)
            => _client.WriteSingleRegister(address, value);
    }
}

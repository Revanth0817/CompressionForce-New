using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyModbus;
using System;

namespace CompressionForce.Integrations.Services
{
    public class ModbusService : IDisposable
    {
        private ModbusClient _client;

        public void Connect(string ip, int port = 502)
        {
            if (_client != null && _client.Connected)
                return;

            _client = new ModbusClient(ip, port);
            _client.Connect();
        }

        public int[] ReadHoldingRegisters(int startAddress, int count)
        {
            EnsureConnected();
            return _client.ReadHoldingRegisters(startAddress, count);
        }

        public void WriteSingleRegister(int address, int value)
        {
            EnsureConnected();
            _client.WriteSingleRegister(address, value);
        }

        private void EnsureConnected()
        {
            if (_client == null || !_client.Connected)
                throw new InvalidOperationException("Modbus client is not connected.");
        }

        public void Dispose()
        {
            if (_client != null && _client.Connected)
                _client.Disconnect();
        }
    }
}

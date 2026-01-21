using EasyModbus;
using System;

namespace CompressionForce.Domain.Plc
{
    public class PlcModbusClient : IDisposable
    {
        private readonly ModbusClient _client;
        private readonly object _sync = new();
        private bool _disposed;

        public PlcModbusClient(string ip, int port)
        {
            _client = new ModbusClient(ip, port)
            {
                ConnectionTimeout = 3000
            };
        }

        // =========================
        // CONNECTION HANDLING
        // =========================

        private void EnsureConnected()
        {
            if (_client.Connected)
                return;

            try
            {
                _client.Connect();
            }
            catch
            {
                SafeDisconnect();
                throw;
            }
        }

        private void Reconnect()
        {
            SafeDisconnect();
            _client.Connect();
        }

        private void SafeDisconnect()
        {
            try
            {
                if (_client.Connected)
                    _client.Disconnect();
            }
            catch { }
        }

        // =========================
        // READ METHODS (SAFE)
        // =========================

        public int[] ReadInputRegisters(int start, int count)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    return _client.ReadInputRegisters(start, count);
                }
                catch
                {
                    Reconnect();
                    return _client.ReadInputRegisters(start, count);
                }
            }
        }

        public int[] ReadHoldingRegisters(int start, int count)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    return _client.ReadHoldingRegisters(start, count);
                }
                catch
                {
                    Reconnect();
                    return _client.ReadHoldingRegisters(start, count);
                }
            }
        }

        public bool[] ReadDiscreteInputs(int start, int count)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    return _client.ReadDiscreteInputs(start, count);
                }
                catch
                {
                    Reconnect();
                    return _client.ReadDiscreteInputs(start, count);
                }
            }
        }

        public bool[] ReadCoils(int start, int count)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    return _client.ReadCoils(start, count);
                }
                catch
                {
                    Reconnect();
                    return _client.ReadCoils(start, count);
                }
            }
        }

        // =========================
        // WRITE METHODS (FIXED 🔥)
        // =========================

        public void WriteSingleCoil(int address, bool value)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    _client.WriteSingleCoil(address, value);
                }
                catch
                {
                    Reconnect();
                    _client.WriteSingleCoil(address, value);
                }
            }
        }

        public void WriteSingleRegister(int address, int value)
        {
            lock (_sync)
            {
                try
                {
                    EnsureConnected();
                    _client.WriteSingleRegister(address, value);
                }
                catch
                {
                    Reconnect();
                    _client.WriteSingleRegister(address, value);
                }
            }
        }

        // =========================
        // CLEANUP
        // =========================

        public void Dispose()
        {
            if (_disposed) return;
            SafeDisconnect();
            _disposed = true;
        }
    }
}

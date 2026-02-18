using EasyModbus;
using CompressionForce.Domain.PLC;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.PLC.Modbus
{
    public class ModbusTcpProtocol : IPlcProtocol
    {
        private readonly ModbusClient _client;
        private readonly object _lock = new();
        private static readonly SemaphoreSlim _plcLock = new SemaphoreSlim(1, 1);

        private readonly string _ip;
        private readonly int _port;

        public ModbusTcpProtocol(string ip, int port)
        {
            _ip = ip;
            _port = port;

            _client = new ModbusClient(ip, port)
            {
                ConnectionTimeout = 3000,
                UnitIdentifier = 1
            };
        }

        // =====================================
        // SAFE CONNECT
        // =====================================
        private void EnsureConnected()
        {
            lock (_lock)
            {
                if (!_client.Connected)
                {
                    try
                    {
                        Console.WriteLine("PLC reconnecting...");
                        _client.Connect();
                        Console.WriteLine("PLC connected.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"PLC reconnect failed: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        private void SafeReconnect()
        {
            try
            {
                if (_client.Connected)
                    _client.Disconnect();
            }
            catch { }

            Thread.Sleep(200);

            try
            {
                _client.Connect();
                Console.WriteLine("PLC reconnected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC reconnection failed: {ex.Message}");
                throw;
            }
        }

        // =====================================
        // CONNECTION
        // =====================================
        public async Task ConnectAsync()
        {
            await _plcLock.WaitAsync();
            try
            {
                EnsureConnected();
            }
            finally
            {
                _plcLock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _plcLock.WaitAsync();
            try
            {
                if (_client.Connected)
                    _client.Disconnect();
            }
            finally
            {
                _plcLock.Release();
            }
        }

        // =====================================
        // READ METHODS
        // =====================================
        public async Task<bool> ReadDiscreteInputAsync(int address)
        {
            return await ExecuteAsync(() =>
                _client.ReadDiscreteInputs(address, 1)[0]);
        }

        public async Task<bool> ReadCoilAsync(int address)
        {
            return await ExecuteAsync(() =>
                _client.ReadCoils(address, 1)[0]);
        }

        public async Task<int> ReadInputRegisterAsync(int address)
        {
            return await ExecuteAsync(() =>
                _client.ReadInputRegisters(address, 1)[0]);
        }

        public async Task<int> ReadHoldingRegisterAsync(int address)
        {
            return await ExecuteAsync(() =>
                _client.ReadHoldingRegisters(address, 1)[0]);
        }

        // =====================================
        // WRITE METHODS
        // =====================================
        public async Task WriteCoilAsync(int address, bool value)
        {
            await ExecuteAsync(() =>
            {
                _client.WriteSingleCoil(address, value);
                return true;
            });
        }

        public async Task WriteHoldingRegisterAsync(int address, int value)
        {
            await ExecuteAsync(() =>
            {
                _client.WriteSingleRegister(address, value);
                return true;
            });
        }

        // =====================================
        // SAFE EXECUTOR
        // =====================================
        private async Task<T> ExecuteAsync<T>(Func<T> action)
        {
            await _plcLock.WaitAsync();

            try
            {
                EnsureConnected();
                return action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC Error: {ex.Message}");

                try
                {
                    SafeReconnect();
                    return action();
                }
                catch
                {
                    Console.WriteLine("PLC recovery failed.");
                    throw;
                }
            }
            finally
            {
                _plcLock.Release();
            }
        }
    }
}

using CompressionForce.Domain.PLC;
using TwinCAT.Ads;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.PLC.Ads
{
    public class AdsTcpProtocol : IPlcProtocol, IDisposable
    {
        private readonly AdsClient _client = new();
        private readonly string _amsNetId;
        private readonly int _port;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        // ✅ Cache handles — avoids 2 extra ADS calls per read
        private readonly ConcurrentDictionary<string, uint> _handleCache = new();

        public AdsTcpProtocol(string amsNetId, int port)
        {
            _amsNetId = amsNetId;
            _port = port;
        }

        // =====================================
        // INTERFACE PROPERTIES
        // =====================================
        public bool IsConnected => _client.IsConnected;
        public string Host => _amsNetId;
        public bool SupportsSymbolPath => true;

        // =====================================
        // CONNECTION
        // =====================================
        public async Task ConnectAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!_client.IsConnected)
                {
                    _handleCache.Clear();
                    _client.Connect(_amsNetId, _port);
                    Console.WriteLine($"✅ ADS connected to {_amsNetId}:{_port}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ADS connect failed: {ex.Message}");
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _lock.WaitAsync();
            try
            {
                _handleCache.Clear();
                if (_client.IsConnected)
                    _client.Disconnect();
            }
            finally
            {
                _lock.Release();
            }
        }

        // =====================================
        // HANDLE CACHE
        // =====================================
        private uint GetOrCreateHandle(string symbolPath)
        {
            return _handleCache.GetOrAdd(symbolPath, path =>
                _client.CreateVariableHandle(path)
            );
        }

        private void InvalidateHandles()
        {
            _handleCache.Clear();
        }

        // =====================================
        // SYMBOL-BASED READS (PREFERRED FOR ADS)
        // =====================================
        public async Task<bool> ReadBoolAsync(string symbolPath)
        {
            return await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = new byte[1];
                _client.Read(handle, buf.AsMemory());
                return buf[0] != 0;
            });
        }

        public async Task<short> ReadIntAsync(string symbolPath)
        {
            return await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = new byte[2];
                _client.Read(handle, buf.AsMemory());
                return BitConverter.ToInt16(buf, 0);
            });
        }

        // =====================================
        // REAL (FLOAT) — 4 bytes for TwinCAT REAL
        // =====================================
        public async Task<float> ReadFloatAsync(string symbolPath)
        {
            return await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = new byte[4];
                _client.Read(handle, buf.AsMemory());
                return BitConverter.ToSingle(buf, 0);
            });
        }

        // =====================================
        // SYMBOL-BASED WRITES
        // =====================================
        public async Task WriteBoolAsync(string symbolPath, bool value)
        {
            await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = new byte[] { value ? (byte)1 : (byte)0 };
                _client.Write(handle, buf.AsMemory());
                return true;
            });
        }

        public async Task WriteIntAsync(string symbolPath, short value)
        {
            await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = BitConverter.GetBytes(value);
                _client.Write(handle, buf.AsMemory());
                return true;
            });
        }

        public async Task WriteFloatAsync(string symbolPath, float value)
        {
            await ExecuteAsync(() =>
            {
                var handle = GetOrCreateHandle(symbolPath);
                var buf = BitConverter.GetBytes(value);
                _client.Write(handle, buf.AsMemory());
                return true;
            });
        }

        // =====================================
        // ADDRESS-BASED READS (FALLBACK)
        // =====================================
        private const uint IG_M_AREA = 0x4020;
        private const uint IG_I_AREA = 0xF020;

        public async Task<bool> ReadDiscreteInputAsync(int address)
        {
            return await ExecuteAsync(() =>
            {
                var buf = new byte[1];
                _client.Read(IG_M_AREA, (uint)address, buf.AsMemory());
                return buf[0] != 0;
            });
        }

        public async Task<bool> ReadCoilAsync(int address)
        {
            return await ExecuteAsync(() =>
            {
                var buf = new byte[1];
                _client.Read(IG_M_AREA, (uint)address, buf.AsMemory());
                return buf[0] != 0;
            });
        }

        public async Task<int> ReadInputRegisterAsync(int address)
        {
            return await ExecuteAsync(() =>
            {
                var buf = new byte[2];
                _client.Read(IG_I_AREA, (uint)(address * 2), buf.AsMemory());
                return (int)BitConverter.ToInt16(buf, 0);
            });
        }

        public async Task<int> ReadHoldingRegisterAsync(int address)
        {
            return await ExecuteAsync(() =>
            {
                var buf = new byte[2];
                _client.Read(IG_M_AREA, (uint)(address * 2), buf.AsMemory());
                return (int)BitConverter.ToInt16(buf, 0);
            });
        }

        // =====================================
        // ADDRESS-BASED WRITES (FALLBACK)
        // =====================================
        public async Task WriteCoilAsync(int address, bool value)
        {
            await ExecuteAsync(() =>
            {
                var buf = new byte[] { value ? (byte)1 : (byte)0 };
                _client.Write(IG_M_AREA, (uint)address, buf.AsMemory());
                return true;
            });
        }

        public async Task WriteHoldingRegisterAsync(int address, int value)
        {
            await ExecuteAsync(() =>
            {
                var buf = BitConverter.GetBytes((short)value);
                _client.Write(IG_M_AREA, (uint)(address * 2), buf.AsMemory());
                return true;
            });
        }

        // =====================================
        // SAFE EXECUTOR
        // =====================================
        private async Task<T> ExecuteAsync<T>(Func<T> action)
        {
            await _lock.WaitAsync();
            try
            {
                if (!_client.IsConnected)
                {
                    InvalidateHandles();
                    _client.Connect(_amsNetId, _port);
                    Console.WriteLine("✅ ADS reconnected.");
                }

                return action();
            }
            catch (AdsErrorException)
            {
                // Handle may be stale after PLC restart — clear and retry
                InvalidateHandles();
                try
                {
                    if (_client.IsConnected) _client.Disconnect();
                    _client.Connect(_amsNetId, _port);
                    return action();
                }
                catch
                {
                    Console.WriteLine("❌ ADS recovery failed.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ADS error: {ex.Message}");
                InvalidateHandles();
                try
                {
                    if (_client.IsConnected) _client.Disconnect();
                    _client.Connect(_amsNetId, _port);
                    return action();
                }
                catch
                {
                    Console.WriteLine("❌ ADS recovery failed.");
                    throw;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        // =====================================
        // DISPOSE
        // =====================================
        public void Dispose()
        {
            _handleCache.Clear();
            _client?.Dispose();
        }
    }
}
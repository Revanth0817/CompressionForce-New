using TwinCAT.Ads;
using CompressionForce.Domain.PLC;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CompressionForce.Integrations.PLC.Ads
{
    public class AdsProtocol : IPlcProtocol
    {
        private readonly string _amsNetId;
        private readonly int _adsPort;
        private readonly PlcTagConfig _config;
        private readonly AdsClient _client;

        public AdsProtocol(string amsNetId, int adsPort, PlcTagConfig config)
        {
            _amsNetId = amsNetId;
            _adsPort = adsPort;
            _config = config;
            _client = new AdsClient();
        }

        public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                _client.Connect(_amsNetId, _adsPort);
                Console.WriteLine("✅ ADS Connected Successfully");
            }

            await Task.CompletedTask;
        }

        public async Task DisconnectAsync()
        {
            if (_client.IsConnected)
            {
                _client.Dispose();
                Console.WriteLine("🔌 ADS Disconnected");
            }

            await Task.CompletedTask;
        }

        private PlcTag GetTagByAddress(int address)
        {
            var tag = _config.Tags.FirstOrDefault(t => t.Address == address);

            if (tag == null)
                throw new Exception($"PLC tag not found for address: {address}");

            if (string.IsNullOrWhiteSpace(tag.Symbol))
                throw new Exception($"ADS Symbol missing for tag: {tag.Key}");

            return tag;
        }

        public async Task<bool> ReadCoilAsync(int address)
        {
            var tag = GetTagByAddress(address);
            var value = (bool)_client.ReadValue(tag.Symbol!);
            return await Task.FromResult(value);
        }

        public async Task<bool> ReadDiscreteInputAsync(int address)
        {
            var tag = GetTagByAddress(address);
            var value = (bool)_client.ReadValue(tag.Symbol!);
            return await Task.FromResult(value);
        }

        public async Task<int> ReadInputRegisterAsync(int address)
        {
            var tag = GetTagByAddress(address);
            var value = Convert.ToInt32(_client.ReadValue(tag.Symbol!));
            return await Task.FromResult(value);
        }

        public async Task<int> ReadHoldingRegisterAsync(int address)
        {
            var tag = GetTagByAddress(address);
            var value = Convert.ToInt32(_client.ReadValue(tag.Symbol!));
            return await Task.FromResult(value);
        }

        public async Task WriteCoilAsync(int address, bool value)
        {
            var tag = GetTagByAddress(address);
            _client.WriteValue(tag.Symbol!, value);
            await Task.CompletedTask;
        }

        public async Task WriteHoldingRegisterAsync(int address, int value)
        {
            var tag = GetTagByAddress(address);
            _client.WriteValue(tag.Symbol!, value);
            await Task.CompletedTask;
        }
    }
}
using CompressionForce.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Plc
{
    public sealed class PlcWriteConfirmService : IPlcWriteConfirmService
    {
        private readonly IPlcSignalWriter _writer;
        private readonly PlcSignalCache _cache;

        public PlcWriteConfirmService(
            IPlcSignalWriter writer,
            PlcSignalCache cache)
        {
            _writer = writer;
            _cache = cache;
        }

        public async Task WriteAndConfirmAsync(
            string key,
            object value,
            TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(2);

            await _writer.WriteAsync(key, value);

            var start = DateTime.UtcNow;

            while (DateTime.UtcNow - start < timeout)
            {
                if (_cache.TryGet(key, out var cached) &&
                    Equals(cached, value))
                    return;

                await Task.Delay(50);
            }

            throw new TimeoutException($"PLC write confirm failed for {key}");
        }
    }
}

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

            foreach (var s in signals)
                result[s.SignalId] = 0;

            return Task.FromResult<IDictionary<string, object>>(result);
        }

        public Task WriteAsync(string signalId, object value)
        {
            return Task.CompletedTask;
        }
    }
}

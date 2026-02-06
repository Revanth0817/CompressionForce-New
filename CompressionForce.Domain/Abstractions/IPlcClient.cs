using System.Collections.Generic;
using System.Threading.Tasks;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain
    .Abstractions
{
    public interface IPlcClient
    {
        Task ConnectAsync();
        Task DisconnectAsync();

        Task<IDictionary<string, object>> ReadAsync(IEnumerable<PlcSignal> signals);
        Task WriteAsync(string signalId, object value);
    }
}

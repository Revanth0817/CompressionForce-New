using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;

namespace CompressionForce.Services.Plc
{
    public sealed class PlcWriteService : IPlcWriteService
    {
        private readonly IPlcClient _client;

        public PlcWriteService(IPlcClient client)
        {
            _client = client;
        }

        public Task WriteAsync(string signalId, object value)
        {
            return _client.WriteAsync(signalId, value);
        }
    }
}

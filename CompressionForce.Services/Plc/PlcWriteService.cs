using System.Threading.Tasks;
using CompressionForce.Domain.Abstractions;
using CompressionForce.Services.Interfaces;

namespace CompressionForce.Services.Plc
{
    public sealed class PlcReadService : IPlcReadService
    {
        private readonly IPlcSignalRegistry _registry;
        private readonly IPlcSignalCache _cache;


        public object Read(string referenceName)
        {
            var reference = _registry.ResolveReference(referenceName);

            // Always read from primary cache
            return _cache.Get(reference.Primary.SignalId);
        }
    }
}

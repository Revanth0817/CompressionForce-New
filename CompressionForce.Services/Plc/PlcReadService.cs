using CompressionForce.Domain.Abstractions;
using CompressionForce.Services.Interfaces;

namespace CompressionForce.Services
{
    public sealed class PlcReadService : IPlcReadService
    {
        private readonly IPlcSignalRegistry _registry;
        private readonly IPlcSignalCache _cache;

        public PlcReadService(
            IPlcSignalRegistry registry,
            IPlcSignalCache cache)
        {
            _registry = registry;
            _cache = cache;
        }

        public object Read(string referenceName)
        {
            var reference = _registry.ResolveReference(referenceName);

            // Always read from primary signal cache
            var signalValue = _cache.Get(reference.Primary.SignalId);

            return signalValue.Value;
        }

    }
}

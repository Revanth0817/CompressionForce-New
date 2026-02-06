using CompressionForce.Domain.Abstractions;

namespace CompressionForce.Services.Plc
{
    public sealed class PlcReadService : IPlcReadService
    {
        private readonly IPlcSignalCache _cache;

        public PlcReadService(IPlcSignalCache cache)
        {
            _cache = cache;
        }

        public T Read<T>(string signalId)
        {
            var value = _cache.Get(signalId);
            return value == null ? default : (T)value.Value;
        }
    }
}

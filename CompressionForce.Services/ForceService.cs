using CompressionForce.Domain.PLC;

namespace CompressionForce.Services
{
    public class ForceService
    {
        private readonly PlcMemoryCache _cache;

        public ForceService(PlcMemoryCache cache)
        {
            _cache = cache;
        }

        public double GetForce()
        {
            int raw = _cache.Get<int>("PRESSURE_RAW");

            // Simple scaling (replace with real logic later)
            double factor = 0.05;
            double offset = 0;

            return raw * factor + offset;
        }
    }
}

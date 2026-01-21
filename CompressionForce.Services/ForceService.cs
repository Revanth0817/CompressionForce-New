using CompressionForce.Domain.Calibration;

namespace CompressionForce.Services
{
    public class ForceService
    {
        private readonly PlcMemoryCache _cache;
        private readonly CalibrationCurve _curve;

        public ForceService(
            PlcMemoryCache cache,
            CalibrationCurve curve)
        {
            _cache = cache;
            _curve = curve;
        }

        public double GetForce()
        {
            // ✅ Explicit generic type
            int raw = _cache.Get<int>("PRESSURE_RAW");

            return _curve.CalculateConcentration(raw);
        }
    }
}

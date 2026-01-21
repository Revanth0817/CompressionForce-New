using System.Collections.Concurrent;

namespace CompressionForce.Domain.Calibration
{
    public class CalibrationRuntimeService
    {
        private readonly ConcurrentDictionary<string, LoadCellCalibrationRuntime> _cache
            = new();

        public void Set(string key, double factor, double offset)
        {
            Console.WriteLine("Its Storing here --> Key: " + key, " Offset: " + offset, " Factor: " + factor);
            _cache[key] = new LoadCellCalibrationRuntime
            {
                
                Factor = factor,
                Offset = offset
            };
        }

        public bool TryGet(string key, out LoadCellCalibrationRuntime cal)
        {
            return _cache.TryGetValue(key, out cal);
        }
    }
}

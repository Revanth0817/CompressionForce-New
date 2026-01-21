using CompressionForce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Calibration
{
    public interface ICalibrationRepository
    {
        // 🔥 REQUIRED
        Task<List<LoadCellCalibration>> GetAllAsync();

        // (optional but recommended)
        LoadCellCalibration? GetLatest(string loadCellCode);
    }
}

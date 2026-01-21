using CompressionForce.Domain.Calibration;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompressionForce.Data
{
    public class CalibrationRepository : ICalibrationRepository
    {
        private readonly ApplicationDbContext _context;

        public CalibrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoadCellCalibration>> GetAllAsync()
        {
            return await _context.LoadCellCalibrations
                .AsNoTracking()
                .ToListAsync();
        }

        public LoadCellCalibration? GetLatest(string loadCellCode)
        {
            return _context.LoadCellCalibrations
                .AsNoTracking()
                .Where(x => x.LoadCellCode == loadCellCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();
        }
    }
}

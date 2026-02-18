using CompressionForce.Data;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Services.Lookups
{
    /// <summary>
    /// Reads lookup values from database.
    /// </summary>
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _dbContext;

        public LookupService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<string>> GetCodesAsync(string category)
        {
            return await _dbContext.LookupValues
                .Where(l => l.Category == category && l.IsActive)
                .Select(l => l.Code)
                .ToListAsync();
        }
    }
}

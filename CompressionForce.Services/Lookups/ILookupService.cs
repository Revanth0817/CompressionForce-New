using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Lookups
{
    /// <summary>
    /// Application service for lookup values.
    /// </summary>
    public interface ILookupService
    {
        Task<IReadOnlyList<string>> GetCodesAsync(string category);
    }
}

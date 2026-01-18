using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{


    public interface ICurrentBatchRepository
    {
        Task<CurrentBatch> GetByBatchCodeAsync(string batchCode);
        Task AddAsync(CurrentBatch batch);
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompressionForce.Services.DTOs.Requests;
using CompressionForce.Services.DTOs.Batch;

namespace CompressionForce.Services.Interfaces
{
    public interface IBatchApplicationService
    {
        Task AddBatchAsync(AddBatchRequest request);
        Task EditBatchAsync(EditBatchRequest request);
        Task DeactivateBatchAsync(DeactivateBatchRequest request);
    }
}

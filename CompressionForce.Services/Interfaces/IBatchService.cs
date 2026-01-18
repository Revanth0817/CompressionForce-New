using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.Interfaces
{
    public interface IBatchService
    {
        void AddBatch(string recipeCode, string batchCode, int batchQty, int tabletQty);
        void UpdateBatchQty(string batchCode, int batchQty);
        void DeactivateBatch(string batchCode);
    }
}

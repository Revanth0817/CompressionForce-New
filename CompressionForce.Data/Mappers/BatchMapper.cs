using CompressionForce.Data.Entities;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Data.Mappers
{
    public static class BatchMapper
    {
        // ============================
        // Batch Mapping
        // ============================

        public static Batch ToDomain(BatchEntity entity)
        {
            if (entity == null) return null!;

            return new Batch
            {
                Id = entity.Id,
                DateTime = entity.DateTime,
                UserName = entity.UserName,
                RecipeCode = entity.RecipeCode,
                BatchCode = entity.BatchCode,
                BatchQty = entity.BatchQty,
                TabletQty = entity.TabletQty,
                BatchStatus = entity.BatchStatus,
                GoodQty = entity.GoodQty,
                RejectionQty = entity.RejectionQty,
                BatchSize = entity.BatchSize,
                BatchNumber = entity.BatchNumber,
                ProdusedQty = entity.ProdusedQty,
                LeftQty = entity.LeftQty,
                S2GoodQty = entity.S2GoodQty,
                S2RejectionQty = entity.S2RejectionQty,
                BatchCondition = entity.BatchCondition
            };
        }

        public static BatchEntity ToEntity(Batch domain)
        {
            if (domain == null) return null!;

            return new BatchEntity
            {
                Id = domain.Id,
                DateTime = domain.DateTime,
                UserName = domain.UserName,
                RecipeCode = domain.RecipeCode,
                BatchCode = domain.BatchCode,
                BatchQty = domain.BatchQty,
                TabletQty = domain.TabletQty,
                BatchStatus = domain.BatchStatus,
                GoodQty = domain.GoodQty,
                RejectionQty = domain.RejectionQty,
                BatchSize = domain.BatchSize,
                BatchNumber = domain.BatchNumber,
                ProdusedQty = domain.ProdusedQty,
                LeftQty = domain.LeftQty,
                S2GoodQty = domain.S2GoodQty,
                S2RejectionQty = domain.S2RejectionQty,
                BatchCondition = domain.BatchCondition
            };
        }

        public static void ToEntity(Batch source, BatchEntity target)
        {
            // Business fields only — never touch Id
            target.DateTime = source.DateTime;
            target.UserName = source.UserName;
            target.RecipeCode = source.RecipeCode;
            target.BatchCode = source.BatchCode;
            target.BatchQty = source.BatchQty;
            target.TabletQty = source.TabletQty;
            target.BatchStatus = source.BatchStatus;
            target.GoodQty = source.GoodQty;
            target.RejectionQty = source.RejectionQty;

            target.BatchSize = source.BatchSize;
            target.BatchNumber = source.BatchNumber;
            target.ProdusedQty = source.ProdusedQty;
            target.LeftQty = source.LeftQty;
            target.S2GoodQty = source.S2GoodQty;
            target.S2RejectionQty = source.S2RejectionQty;
            target.BatchCondition = source.BatchCondition;
        }

        public static void ToEntity(CurrentBatch source, CurrentBatchEntity target)
        {
            target.DateTime = source.DateTime;
            target.BatchNumber = source.BatchNumber;
            target.Parameters = source.Parameters;
        }
        // ============================
        // BatchHistory Mapping
        // ============================

        public static BatchHistory ToDomain(BatchHistoryEntity entity)
        {
            if (entity == null) return null!;

            return new BatchHistory
            {
                Id = entity.Id,
                DateTime = entity.DateTime,
                RecipeCode = entity.RecipeCode,
                BatchCode = entity.BatchCode,
                UserName = entity.UserName,
                BatchSize = entity.BatchSize,
                EReventType = entity.EReventType
            };
        }

        public static BatchHistoryEntity ToEntity(BatchHistory domain)
        {
            if (domain == null) return null!;

            return new BatchHistoryEntity
            {
                Id = domain.Id,
                DateTime = domain.DateTime,
                RecipeCode = domain.RecipeCode,
                BatchCode = domain.BatchCode,
                UserName = domain.UserName,
                BatchSize = domain.BatchSize,
                EReventType = domain.EReventType
            };
        }

        // ============================
        // CurrentBatch Mapping
        // ============================

        public static CurrentBatch ToDomain(CurrentBatchEntity entity)
        {
            if (entity == null) return null!;

            return new CurrentBatch
            {
                Id = entity.Id,
                DateTime = entity.DateTime,
                BatchNumber = entity.BatchNumber,
                Parameters = entity.Parameters
            };
        }

        public static CurrentBatchEntity ToEntity(CurrentBatch domain)
        {
            if (domain == null) return null!;

            return new CurrentBatchEntity
            {
                Id = domain.Id,
                DateTime = domain.DateTime,
                BatchNumber = domain.BatchNumber,
                Parameters = domain.Parameters
            };
        }
    }
}

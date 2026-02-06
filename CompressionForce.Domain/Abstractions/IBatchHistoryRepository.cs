using CompressionForce.Domain.Entities;

namespace CompressionForce.Domain.Abstractions
{
    public interface IBatchHistoryRepository
    {
        Task EntryAsync(Batch batch, string eventname);


    }

}


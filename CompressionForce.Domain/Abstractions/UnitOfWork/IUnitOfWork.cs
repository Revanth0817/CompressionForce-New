namespace CompressionForce.Domain.Abstractions.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

}

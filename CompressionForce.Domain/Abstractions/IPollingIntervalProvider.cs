using CompressionForce.Domain.Enums;

namespace CompressionForce.Domain.Abstractions
{
    public interface IPollingIntervalProvider
    {
        int GetIntervalMilliseconds(UpdateClass updateClass);
    }
}

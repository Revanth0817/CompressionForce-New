using CompressionForce.Domain.Plc;

namespace CompressionForce.Domain.Abstractions
{
    public interface IPlcBatchReader
    {
        IDictionary<string, object> ReadBatch(PlcPollBatch batch);
    }
}

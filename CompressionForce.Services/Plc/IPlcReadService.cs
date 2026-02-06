namespace CompressionForce.Services.Plc
{
    public interface IPlcReadService
    {
        T Read<T>(string signalId);
    }
}

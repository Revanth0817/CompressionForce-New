using System.Threading.Tasks;

namespace CompressionForce.Services.Plc
{
    public interface IPlcWriteService
    {
        Task WriteAsync(string signalId, object value);
    }
}

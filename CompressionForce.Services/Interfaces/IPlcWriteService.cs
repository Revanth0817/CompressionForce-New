using System.Threading.Tasks;

namespace CompressionForce.Services.Interfaces
{
    public interface IPlcWriteService
    {
        Task WriteAsync(string signalId, object value);
    }
}

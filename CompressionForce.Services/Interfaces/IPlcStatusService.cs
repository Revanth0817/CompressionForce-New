using CompressionForce.Services.DTOs;

namespace CompressionForce.Services.Interfaces
{
    public interface IPlcStatusService
    {
        PlcStatusVm GetPlcStatus();
    }
}

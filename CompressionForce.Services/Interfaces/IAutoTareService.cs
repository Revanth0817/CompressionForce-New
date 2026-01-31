using CompressionForce.Services.DTOs;
using CompressionForce.Services.DTOs.Batch;
//using CompressionForce.Services.DTOs.Responses;

namespace CompressionForce.Services.Interfaces
{
    public interface IAutoTareService
    {
        AutoTareDto GetSnapshot();
    }
}
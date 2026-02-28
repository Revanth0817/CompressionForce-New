using CompressionForce.Domain.PLC;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcHealthController : ControllerBase
    {
        private readonly IPlcProtocol _plc;

        public PlcHealthController(IPlcProtocol plc)
        {
            _plc = plc;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                connected = _plc.IsConnected,
                host = _plc.Host,
                supportsSymbolPath = _plc.SupportsSymbolPath,
                timestamp = DateTime.Now
            });
        }

        [HttpGet("test-read")]
        public async Task<IActionResult> TestRead()
        {
            try
            {
                // Try reading the first bool symbol
                var result = await _plc.ReadBoolAsync("GVL_IO.bDI_Emergency");

                return Ok(new
                {
                    success = true,
                    symbolPath = "GVL_IO.bDI_Emergency",
                    value = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}
using CompressionForce.Domain.PLC;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CompressionForce.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcWriteController : ControllerBase
    {
        private readonly IPlcProtocol _plc;
        private readonly PlcTagConfig _config;

        public PlcWriteController(IPlcProtocol plc, PlcTagConfig config)
        {
            _plc = plc;
            _config = config;
        }

        /// <summary>
        /// Write a coil (boolean command) to PLC
        /// </summary>
        [HttpPost("coil")]
        public async Task<IActionResult> WriteCoil([FromBody] WriteCoilRequest request)
        {
            try
            {
                // Find tag by key to get its address
                var tag = _config.Tags.FirstOrDefault(t => t.Key == request.TagKey);
                if (tag == null)
                    return NotFound(new { error = $"Tag '{request.TagKey}' not found" });

                if (tag.Type != PlcDataType.Coil)
                    return BadRequest(new { error = $"Tag '{request.TagKey}' is not a Coil type" });

                // Write to PLC
                await _plc.WriteCoilAsync(tag.Address, request.Value);

                return Ok(new { message = $"Coil {request.TagKey} written successfully", address = tag.Address, value = request.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Write a holding register (numeric command) to PLC
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> WriteHoldingRegister([FromBody] WriteRegisterRequest request)
        {
            try
            {
                var tag = _config.Tags.FirstOrDefault(t => t.Key == request.TagKey);
                if (tag == null)
                    return NotFound(new { error = $"Tag '{request.TagKey}' not found" });

                if (tag.Type != PlcDataType.HoldingRegister)
                    return BadRequest(new { error = $"Tag '{request.TagKey}' is not a HoldingRegister type" });

                await _plc.WriteHoldingRegisterAsync(tag.Address, request.Value);

                return Ok(new { message = $"HoldingRegister {request.TagKey} written successfully", address = tag.Address, value = request.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class WriteCoilRequest
    {
        public string TagKey { get; set; }
        public bool Value { get; set; }
    }

    public class WriteRegisterRequest
    {
        public string TagKey { get; set; }
        public int Value { get; set; }
    }
}
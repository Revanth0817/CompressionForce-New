using CompressionForce.Domain.PLC;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CompressionForce.Web.Controllers
{
    [Route("[controller]")]
    public class DiagnosticsController : Controller
    {
        private readonly IPlcProtocol _plc;
        private readonly PlcTagConfig _tagConfig;

        public DiagnosticsController(
            IPlcProtocol plc,
            PlcTagConfig tagConfig)
        {
            _plc = plc;
            _tagConfig = tagConfig;
        }

        // -------------------------------
        // PAGE
        // -------------------------------
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Diagnostics");
        }

        // -------------------------------
        // DIGITAL OUTPUT WRITE
        // -------------------------------
        [HttpPost("WriteDigitalOutput")]
        public async Task<IActionResult> WriteDigitalOutput(
            [FromBody] DigitalOutputWriteDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TagKey))
                return BadRequest("Invalid payload");

            // 🔹 Resolve tag → address
                var tag = _tagConfig.Tags.FirstOrDefault(t =>
                t.Key == dto.TagKey &&
                t.Type == PlcDataType.Coil
            );

                if (tag == null)
                return NotFound($"Unknown DO tag: {dto.TagKey}");

            // 🔹 Write to PLC
            await _plc.WriteCoilAsync(tag.Address, dto.Value);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    // -------------------------------
    // DTO
    // -------------------------------
    public class DigitalOutputWriteDto
        {
        public string TagKey { get; set; } = string.Empty;
        public bool Value { get; set; }
    }
}

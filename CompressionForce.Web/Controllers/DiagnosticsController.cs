using CompressionForce.Domain.PLC;
using Microsoft.AspNetCore.Mvc;

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

            var tag = _tagConfig.Tags.FirstOrDefault(t =>
                t.Key == dto.TagKey &&
                t.Type == PlcDataType.Coil
            );

            if (tag == null)
                return NotFound($"Unknown DO tag: {dto.TagKey}");

            // ✅ Use symbolPath for ADS, fallback to address for Modbus
            if (_plc.SupportsSymbolPath && !string.IsNullOrWhiteSpace(tag.SymbolPath))
            {
                await _plc.WriteBoolAsync(tag.SymbolPath, dto.Value);
            }
            else
            {
                await _plc.WriteCoilAsync(tag.Address, dto.Value);
            }

            return Ok();
        }
    }

    public class DigitalOutputWriteDto
    {
        public string TagKey { get; set; }
        public bool Value { get; set; }
    }
}

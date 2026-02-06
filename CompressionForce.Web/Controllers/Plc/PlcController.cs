using Microsoft.AspNetCore.Mvc;
using CompressionForce.Services.Interfaces;

namespace CompressionForce.Web.Controllers.Plc
{
    [ApiController]
    [Route("api/plc")]
    public sealed class PlcController : ControllerBase
    {
        private readonly IPlcReadService _readService;

        public PlcController(IPlcReadService readService)
        {
            _readService = readService;
        }

        [HttpGet("{signalId}")]
        public IActionResult Get(string signalId)
        {
            var signal = _readService.Read(signalId);

            if (signal == null)
                return NotFound();

            return Ok(signal);
        }
    }
}

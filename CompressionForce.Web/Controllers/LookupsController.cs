using CompressionForce.Services.Lookups;
using Microsoft.AspNetCore.Mvc;
using CompressionForce.Web.DTOs;
namespace CompressionForce.Web.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupsController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        // Used by UI for dropdowns (ToolType, Treatment, etc.)
        [HttpGet("{category}")]
        public async Task<IActionResult> Get(string category)
        {
            var values = await _lookupService.GetCodesAsync(category);

            var result = values.Select(v => new LookupDto
            {
                Code = v,
                DisplayName = v // UI can map display names later if needed
            });

            return Ok(result);
        }
    }
}

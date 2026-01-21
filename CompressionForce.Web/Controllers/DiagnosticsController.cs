using Microsoft.AspNetCore.Mvc;
using CompressionForce.Services;
using System;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly DiagnosticsService _diagnosticsService;
        private readonly PlcService _plcService;

        public DiagnosticsController(
            DiagnosticsService diagnosticsService,
            PlcService plcService)
        {
            _diagnosticsService = diagnosticsService;
            _plcService = plcService;
        }

        // =========================
        // PAGE LOAD
        // =========================
        public IActionResult Index()
        {
            return View(_diagnosticsService.Read());
        }

        // =========================
        // UI → PLC DIGITAL OUTPUT WRITE
        // =========================
        [HttpPost]
        public IActionResult SetOutput([FromBody] PlcWriteDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Key))
                return BadRequest("Invalid PLC write request");

            _diagnosticsService.WriteDigitalOutput(dto.Key, dto.Value);
            return Ok();
        }

        // =========================
        // LIVE ANALOG INPUTS (RAW → V → kN)
        // =========================
        [HttpGet]
        public IActionResult GetAnalogInputs()
        {
            var result = _plcService.GetAllAnalogInputs()
                .Select(x => new
                {
                    key = x.Key,
                    voltage = x.Voltage,
                    kn = x.Kn
                })
                .ToList();

            return Json(result);
        }
    }

    // =========================
    // DTO
    // =========================
    public class PlcWriteDto
    {
        public string Key { get; set; }
        public bool Value { get; set; }
    }
}

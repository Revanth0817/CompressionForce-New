using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.PLC;
using System;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    public class CalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly PlcTagConfig _plcConfig;

        public CalibrationController(
            ApplicationDbContext context,
            IMemoryCache cache,
            PlcTagConfig plcConfig)
        {
            _context = context;
            _cache = cache;
            _plcConfig = plcConfig;
        }

        // ==============================
        // Load Calibration Page
        // ==============================
        public IActionResult Index()
        {
            return View();
        }

        // ==============================
        // GET: Loadcell list for dropdown
        // ==============================
        [HttpGet]
        public IActionResult GetLoadCells()
        {
            try
            {
                var loadcells = _context.LoadCells
                    .Where(x => x.IsActive)
                    .Select(x => new
                    {
                        loadCellCode = x.LoadCellCode,
                        loadCellName = x.LoadCellName
                    })
                    .ToList();

                return Ok(loadcells);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to load loadcells",
                    detail = ex.Message
                });
            }
        }

        // ==============================
        // POST: Save calibration values
        // ==============================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SaveCalibration([FromBody] LoadCellCalibration model)
        {
            if (model == null)
                return BadRequest(new { error = "Calibration model is null" });

            if (string.IsNullOrWhiteSpace(model.LoadCellCode))
                return BadRequest(new { error = "LoadCellCode is required" });

            var calibration = new LoadCellCalibration
            {
                LoadCellCode = model.LoadCellCode,
                MinVolt = model.MinVolt,
                MaxVolt = model.MaxVolt,
                MinValue = model.MinValue,
                MaxValue = model.MaxValue,
                Factor = model.Factor,
                Offset = model.Offset,
                CreatedAt = DateTime.UtcNow
            };

            _context.LoadCellCalibrations.Add(calibration);
            _context.SaveChanges();

            return Ok(new { message = "Calibration saved successfully" });
        }

        // ==============================
        // GET: Last saved calibration
        // ==============================
        [HttpGet]
        public IActionResult GetLastCalibration(string loadCellCode)
        {
            if (string.IsNullOrWhiteSpace(loadCellCode))
                return BadRequest(new { error = "LoadCellCode is required" });

            var lastCalibration = _context.LoadCellCalibrations
                .Where(x => x.LoadCellCode == loadCellCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (lastCalibration == null)
                return NotFound(new { error = "No calibration found" });

            return Ok(lastCalibration);
        }

        // ==============================
        // GET: Sensor Voltage (FROM CACHE)
        // ==============================
        [HttpGet]
        public IActionResult GetSensorVoltage(string loadCellCode)
        {
            if (string.IsNullOrWhiteSpace(loadCellCode))
                return BadRequest(new { error = "LoadCellCode missing" });

            var tag = _plcConfig.Tags
                .FirstOrDefault(t => t.LoadCellCode == loadCellCode);

            if (tag == null)
            {
                return NotFound(new
                {
                    error = "PLC tag not configured",
                    loadCellCode
                });
            }

            // IMPORTANT: cache is the ONLY voltage source
            if (!_cache.TryGetValue(tag.Key, out double voltage))
                voltage = 0.0;

            return Ok(new { voltage });
        }
    }
}

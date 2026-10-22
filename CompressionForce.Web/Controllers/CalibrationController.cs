using CompressionForce.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using System;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    [RequirePrivilege("Calibration")]
    public class CalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalibrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==============================
        // Load Calibration Page
        // ==============================
        public IActionResult Index()
        {
            // the calibration screen lives under Home/Calibration
            return RedirectToAction("Calibration", "Home");
        }

        // ==============================
        // GET: Loadcell list for dropdown
        // ==============================
        [HttpGet]
        public IActionResult GetLoadCells()
        {
            var loadcells = _context.LoadCells
                .Where(x => x.IsActive)
                .Select(x => new
                {
                    x.LoadCellCode,
                    x.LoadCellName
                })
                .ToList();

            return Json(loadcells);
        }

        // ==============================
        // POST: Save calibration values
        // ==============================
        [HttpPost]
        [IgnoreAntiforgeryToken] // 🔴 REQUIRED FOR FETCH + JSON
        public IActionResult SaveCalibration([FromBody] LoadCellCalibration model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Calibration model is null");

                if (string.IsNullOrWhiteSpace(model.LoadCellCode))
                    return BadRequest("LoadCellCode is required");

                if (!_context.LoadCells.Any(x => x.LoadCellCode == model.LoadCellCode && x.IsActive))
                    return BadRequest("Unknown or inactive load cell");

                // the columns are numeric(10,4): anything bigger would be rejected by the database
                const decimal limit = 999999m;
                if (new[] { model.MinVolt, model.MaxVolt, model.MinValue, model.MaxValue }
                        .Any(v => Math.Abs(v) > limit))
                    return BadRequest("Voltage and force limits must be between -999999 and 999999");

                if (model.MaxVolt == model.MinVolt)
                    return BadRequest("Min Volt and Max Volt cannot be the same");

                // never trust the browser's maths: factor and offset are always derived from the limits
                var factor = (model.MaxValue - model.MinValue) / (model.MaxVolt - model.MinVolt);
                var offset = model.MinValue - (model.MinVolt * factor);

                var calibration = new LoadCellCalibration
                {
                    LoadCellCode = model.LoadCellCode,
                    MinVolt = model.MinVolt,
                    MaxVolt = model.MaxVolt,
                    MinValue = model.MinValue,
                    MaxValue = model.MaxValue,
                    Factor = Math.Round(factor, 8),
                    Offset = Math.Round(offset, 8),
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoadCellCalibrations.Add(calibration);
                _context.SaveChanges();

                return Ok(new { message = "Calibration saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ==============================
        // GET: Last saved calibration
        // ==============================
        [HttpGet]
        public IActionResult GetLastCalibration(string loadCellCode)
        {
            if (string.IsNullOrWhiteSpace(loadCellCode))
                return BadRequest("LoadCellCode is required");

            var lastCalibration = _context.LoadCellCalibrations
                .Where(x => x.LoadCellCode == loadCellCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (lastCalibration == null)
                return NotFound();

            return Json(lastCalibration);
        }
    }
}

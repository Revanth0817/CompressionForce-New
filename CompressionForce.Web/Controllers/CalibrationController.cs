using Microsoft.AspNetCore.Mvc;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using System;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
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
            return View();
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

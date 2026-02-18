using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.PLC;
using Microsoft.AspNetCore.Mvc;

using CompressionForce.Services;
using System;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    public class CalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PlcMemoryCache _cache;

        public CalibrationController(
            ApplicationDbContext context,
            PlcMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetLoadCells()
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

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SaveCalibration([FromBody] LoadCellCalibration model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.LoadCellCode))
                return BadRequest();

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

            // ✅ SAME CACHE AS PLC POLLING
            _cache.Set($"{model.LoadCellCode}_FACTOR", model.Factor);
            _cache.Set($"{model.LoadCellCode}_OFFSET", model.Offset);

            Console.WriteLine(
                $"🧮 CAL SAVED {model.LoadCellCode} F={model.Factor} O={model.Offset}"
            );

            return Ok(new { message = "Calibration saved" });
        }

        [HttpGet]
        public IActionResult GetLastCalibration(string loadCellCode)
        {
            if (string.IsNullOrWhiteSpace(loadCellCode))
                return BadRequest();

            var last = _context.LoadCellCalibrations
                .Where(x => x.LoadCellCode == loadCellCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (last == null)
                return NotFound();

            _cache.Set($"{loadCellCode}_FACTOR", last.Factor);
            _cache.Set($"{loadCellCode}_OFFSET", last.Offset);

            return Ok(last);
        }

        [HttpGet]
        public IActionResult GetSensorVoltage(string loadCellCode)
        {
            if (string.IsNullOrWhiteSpace(loadCellCode))
                return BadRequest();

            double voltage =
                (_cache.Get($"{loadCellCode}_VOLT") as double?) ?? 0.0;

            return Ok(new { voltage });
        }
    }
}

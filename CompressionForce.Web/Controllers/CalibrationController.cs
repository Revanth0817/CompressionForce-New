using CompressionForce.Data;
using CompressionForce.Data.Entities;
using CompressionForce.Domain.Calibration;
using CompressionForce.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CompressionForce.Web.Controllers
{
    public class CalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PlcMappingProvider _mappingProvider;
        private readonly PlcService _plcService;
        private readonly CalibrationRuntimeService _runtime;

        public CalibrationController(
            ApplicationDbContext context,
            PlcMappingProvider mappingProvider,
            PlcService plcService,
            CalibrationRuntimeService runtime)
        {
            _context = context;
            _mappingProvider = mappingProvider;
            _plcService = plcService;
            _runtime = runtime;
        }

        // ==============================
        // PAGE
        // ==============================
        public IActionResult Index()
        {
            return View();
        }

        // ==============================
        // DROPDOWN LOADCELLS
        // ==============================
        [HttpGet]
        public IActionResult GetLoadCells()
        {
            var list = _mappingProvider.Mapping.AnalogInputs.Keys
                .Select(k => new
                {
                    key = k,
                    name = SplitName(k)
                })
                .ToList();

            return Json(list);
        }

        // ==============================
        // LIVE PLC VOLTAGE
        // ==============================
        [HttpGet]
        public IActionResult GetLiveVoltage(string loadCellKey)
        {
            if (string.IsNullOrWhiteSpace(loadCellKey))
                return BadRequest("Loadcell key required");

            int raw = _plcService.ReadAnalogRaw(loadCellKey);
            double voltage = raw * 10.0 / 32767.0;

            return Json(new
            {
                raw,
                voltage = Math.Round(voltage, 3)
            });
        }

        // ==============================
        // LIVE CALIBRATED (RUNTIME)
        // ==============================
        [HttpGet]
        public IActionResult GetLiveCalibrated(string loadCellKey)
        {
            if (string.IsNullOrWhiteSpace(loadCellKey))
                return BadRequest();

            var result = _plcService.ReadAnalogCalibrated(loadCellKey);

            return Json(new
            {
                voltage = result.Voltage,
                kn = result.Kn
            });
        }

        // ==============================
        // SAVE CALIBRATION (FIXED)
        // ==============================
        [HttpPost]
        public IActionResult SaveCalibration([FromBody] SaveCalibrationDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.LoadCellName))
                return BadRequest("Invalid calibration data");

            // 🔥 1️⃣ SAVE TO RUNTIME (double)
            _runtime.Set(dto.LoadCellName, dto.Factor, dto.Offset);

            // 🔹 2️⃣ SAVE TO DB (decimal)
            var entity = new LoadCellCalibration
            {
                LoadCellName = dto.LoadCellName,

               

                Factor = (decimal)dto.Factor,
                Offset = (decimal)dto.Offset,

                UpdatedAt = DateTime.UtcNow
            };

            _context.LoadCellCalibrations.Add(entity);
            _context.SaveChanges();

            return Ok();
        }

        // ==============================
        // LAST CALIBRATION (DB)
        // ==============================
        [HttpGet]
        public IActionResult GetLastCalibration(string loadCellKey)
        {
            if (string.IsNullOrWhiteSpace(loadCellKey))
                return BadRequest();

            var cal = _context.LoadCellCalibrations
                .Where(x => x.LoadCellName == loadCellKey)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefault();

            if (cal == null)
                return Json(null);

            return Json(new
            {
                
                factor = cal.Factor,
                offset = cal.Offset
            });
        }

        // ==============================
        // HELPER
        // ==============================
        private string SplitName(string key)
        {
            return Regex.Replace(key, "([a-z])([A-Z])", "$1 $2");
        }
    }
}

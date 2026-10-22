using CompressionForce.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    [Route("[controller]/[action]")]
    [RequirePrivilege("Calibration")]
    public class ServoCalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServoCalibrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // SAVE SERVO CALIBRATION
        // =============================
        [HttpPost]
        public IActionResult SaveServoCalibration([FromBody] ServoCalibration model)
        {
            if (model == null)
                return BadRequest("Payload is null");

            if (string.IsNullOrWhiteSpace(model.ServoCode))
                return BadRequest("ServoCode missing");

            if (!System.Text.RegularExpressions.Regex.IsMatch(model.ServoCode, @"^[A-Za-z0-9_\-]{1,60}$"))
                return BadRequest("ServoCode has invalid characters");

            if (string.IsNullOrWhiteSpace(model.ServoName) || model.ServoName.Length > 100)
                return BadRequest("ServoName is required (max 100 characters)");

            // the screen never lets these go below zero; keep the database the same way
            const decimal limit = 1000000m;
            if (new[] { model.JogSpeed, model.TorqueLimit, model.SetPosition, model.SetSpeed }
                    .Any(v => v < 0 || v > limit))
                return BadRequest("Servo values must be between 0 and 1,000,000");

            var entity = new ServoCalibration
            {
                ServoCode = model.ServoCode,
                ServoName = model.ServoName,
                JogSpeed = model.JogSpeed,
                TorqueLimit = model.TorqueLimit,
                SetPosition = model.SetPosition,
                SetSpeed = model.SetSpeed
            };

            _context.ServoCalibrations.Add(entity);
            _context.SaveChanges();

            return Ok(new { message = "Servo calibration saved successfully" });
        }

        // =============================
        // GET LAST CALIBRATION
        // =============================
        [HttpGet]
        public IActionResult GetLastServoCalibration(string servoCode)
        {
            var last = _context.ServoCalibrations
                .Where(x => x.ServoCode == servoCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            return Json(last);
        }
    }
}

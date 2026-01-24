using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.Controllers
{
    [Route("[controller]/[action]")]
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

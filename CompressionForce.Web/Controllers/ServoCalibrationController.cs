using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CompressionForce.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class ServoCalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PlcService _plcService;

        public ServoCalibrationController(ApplicationDbContext context, PlcService plcService)
        {
            _context = context;
            _plcService = plcService;
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
        [HttpPost]
        public IActionResult SaveServoCalibration([FromBody] ServoCalibration model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ServoCode))
                return BadRequest("Invalid payload");

            var existing = _context.ServoCalibrations
                .FirstOrDefault(x => x.ServoCode == model.ServoCode);

            if (existing == null)
            {
                existing = new ServoCalibration
                {
                    ServoCode = model.ServoCode,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ServoCalibrations.Add(existing);
            }

            existing.ServoName = model.ServoName;
            existing.JogSpeed = model.JogSpeed;
            existing.TorqueLimit = model.TorqueLimit;
            existing.SetPosition = model.SetPosition;
            existing.SetSpeed = model.SetSpeed;

            _context.SaveChanges();

            // 🚨 THIS LINE CAN FAIL IF MAPPING IS WRONG
            _plcService.ApplyServoCalibration(existing.ServoCode, existing);

            return Ok(new { message = "Calibration saved & applied to PLC" });
        }


        [HttpGet]
        public IActionResult GetLiveTorque(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                return BadRequest(0);

            try
            {
                decimal torque = _plcService.ReadServoActTorque(servoCode);
                return Ok(torque);
            }
            catch (Exception ex)
            {
                // IMPORTANT: do NOT crash polling
                Console.WriteLine(ex.Message);
                return Ok(0); // safe fallback
            }
        }
        [HttpGet]
        public IActionResult GetServoStatus(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                return Ok(new { ready = false, alarm = true });

            try
            {
                var status = _plcService.ReadServoStatus(servoCode);
                return Ok(new { ready = status.Ready, alarm = status.Alarm });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(new { ready = false, alarm = true });
            }
        }







    }
}

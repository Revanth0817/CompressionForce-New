using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.PLC;
using CompressionForce.Services;
using CompressionForce.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CompressionForce.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class ServoCalibrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlcProtocol _plc;
        private readonly PlcTagConfig _plcConfig;
        private readonly PlcMemoryCache _cache;
        private readonly IHubContext<ServoHub> _hub;

        public ServoCalibrationController(
            ApplicationDbContext context,
            IPlcProtocol plc,
            PlcTagConfig plcConfig,
            PlcMemoryCache cache,
            IHubContext<ServoHub> hub)
        {
            _context = context;
            _plc = plc;
            _plcConfig = plcConfig;
            _cache = cache;
            _hub = hub;
        }

        /* ============================================================
           DB : SAVE SERVO CALIBRATION
        ============================================================ */
        [HttpPost]
        public IActionResult SaveServoCalibration([FromBody] ServoCalibration model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ServoCode))
                return BadRequest("Invalid payload");

            model.CreatedAt = DateTime.UtcNow;

            _context.ServoCalibrations.Add(model);
            _context.SaveChanges();

            return Ok(new { message = "Servo calibration saved successfully" });
        }

        /* ============================================================
           DB : LOAD LAST SERVO CALIBRATION
        ============================================================ */
        [HttpGet]
        public IActionResult GetLastServoCalibration(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                return BadRequest("ServoCode missing");

            var last = _context.ServoCalibrations
                .Where(x => x.ServoCode == servoCode)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            return Ok(last);
        }

        /* ============================================================
           PLC TAG RESOLVER
        ============================================================ */
        private PlcTag GetTagOrThrow(string key)
        {
            var tag = _plcConfig.Tags.FirstOrDefault(t => t.Key == key);
            if (tag == null)
                throw new Exception($"PLC tag not found: {key}");

            return tag;
        }

        /* ============================================================
           COIL PULSE (SAFE)
        ============================================================ */
        private async Task PulseCoil(string tagKey)
        {
            var tag = GetTagOrThrow(tagKey);

            await _plc.WriteCoilAsync(tag.Address, true);
            await Task.Delay(150);
            
        }

        /* ============================================================
           JOG CONTROLS
        ============================================================ */
        [HttpPost]
        public async Task<IActionResult> JogUp(string servoCode)
        {
            await PulseCoil($"{servoCode}_JOG_UP");
            await PublishServoStatus(servoCode);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> JogDown(string servoCode)
        {
            await PulseCoil($"{servoCode}_JOG_DOWN");
            await PublishServoStatus(servoCode);
            return Ok();
        }

        /* ============================================================
           RUN / STOP
        ============================================================ */
        [HttpPost]
        public async Task<IActionResult> Run(string servoCode)
        {
            await PulseCoil($"{servoCode}_RUN");
            await PublishServoStatus(servoCode);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Stop(string servoCode)
        {
            await PulseCoil($"{servoCode}_STOP");
            await PublishServoStatus(servoCode);
            return Ok();
        }

        /* ============================================================
           APPLY SET VALUES (SET_POS, SET_SPEED, JOG_SPEED, SET_VALUE)
        ============================================================ */
        [HttpPost]
        public async Task<IActionResult> ApplySet([FromBody] ServoSetDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ServoCode))
                return BadRequest("Invalid payload");

            try
            {
                // 🔹 SET POSITION (scaled x100)
                var posTag = GetTagOrThrow($"{dto.ServoCode}_SET_POS");
                await _plc.WriteHoldingRegisterAsync(
                    posTag.Address,
                    dto.SetPosition
                );

                // 🔹 SET SPEED
                var speedTag = GetTagOrThrow($"{dto.ServoCode}_SET_SPEED");
                await _plc.WriteHoldingRegisterAsync(
                    speedTag.Address,
                    dto.SetSpeed
                );

                // 🔹 JOG SPEED
                var jogTag = GetTagOrThrow($"{dto.ServoCode}_JOG_SPEED");
                await _plc.WriteHoldingRegisterAsync(
                    jogTag.Address,
                    dto.JogSpeed
                );

                // 🔹 SET VALUE
                var setTag = GetTagOrThrow($"{dto.ServoCode}_SET");
                await _plc.WriteHoldingRegisterAsync(
                    setTag.Address,
                    dto.SetValue
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"PLC write failed: {ex.Message}");
            }
        }

        /* ============================================================
           READ CURRENT SET VALUES FROM PLC
        ============================================================ */
        [HttpGet]
        public async Task<IActionResult> GetCurrentSetValues(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                return BadRequest("ServoCode missing");

            try
            {
                var posTag = GetTagOrThrow($"{servoCode}_SET_POS");
                var speedTag = GetTagOrThrow($"{servoCode}_SET_SPEED");
                var jogTag = GetTagOrThrow($"{servoCode}_JOG_SPEED");
                var setTag = GetTagOrThrow($"{servoCode}_SET");

                int rawPos = await _plc.ReadHoldingRegisterAsync(posTag.Address);
                int rawSpeed = await _plc.ReadHoldingRegisterAsync(speedTag.Address);
                int rawJog = await _plc.ReadHoldingRegisterAsync(jogTag.Address);
                int rawSet = await _plc.ReadHoldingRegisterAsync(setTag.Address);

                return Ok(new
                {
                    setPosition = rawPos ,
                    setSpeed = rawSpeed,
                    jogSpeed = rawJog,
                    setValue = rawSet
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"PLC read failed: {ex.Message}");
            }
        }

        /* ============================================================
           LIVE SERVO STATUS (CACHE)
        ============================================================ */
        [HttpGet]
        public IActionResult GetServoStatus(string servoCode)
        {
            bool ready = (_cache.Get($"{servoCode}_READY") as bool?) ?? false;
            bool alarm = (_cache.Get($"{servoCode}_ALARM") as bool?) ?? false;
            int torque = (_cache.Get($"{servoCode}_ACT_TORQUE") as int?) ?? 0;
            int actPos = (_cache.Get($"{servoCode}_ACT_POS") as int?) ?? 0;

            return Ok(new { ready, alarm, torque, actPos });
        }

        /* ============================================================
           SIGNALR PUSH
        ============================================================ */
        private async Task PublishServoStatus(string servoCode)
        {
            bool ready = (_cache.Get($"{servoCode}_READY") as bool?) ?? false;
            bool alarm = (_cache.Get($"{servoCode}_ALARM") as bool?) ?? false;
            int torque = (_cache.Get($"{servoCode}_ACT_TORQUE") as int?) ?? 0;
            int actPos = (_cache.Get($"{servoCode}_ACT_POS") as int?) ?? 0;

            await _hub.Clients.All.SendAsync(
                "ServoStatusUpdated",
                servoCode,
                ready,
                alarm,
                torque,
                actPos
            );
        }
    }
}

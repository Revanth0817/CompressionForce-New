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
           COIL PULSE (SAFE) — ✅ FIXED for ADS
        ============================================================ */
        private async Task PulseCoil(string tagKey)
        {
            var tag = GetTagOrThrow(tagKey);

            if (_plc.SupportsSymbolPath && !string.IsNullOrWhiteSpace(tag.SymbolPath))
            {
                await _plc.WriteBoolAsync(tag.SymbolPath, true);
                await Task.Delay(150);
            }
            else
            {
                await _plc.WriteCoilAsync(tag.Address, true);
                await Task.Delay(150);
            }
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
           APPLY SET VALUES — ✅ FIXED for ADS
        ============================================================ */
        [HttpPost]
        public async Task<IActionResult> ApplySet([FromBody] ServoSetDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ServoCode))
                return BadRequest("Invalid payload");

            try
            {
                await WriteRegisterTag($"{dto.ServoCode}_SET_POS", dto.SetPosition);
                await WriteRegisterTag($"{dto.ServoCode}_SET_SPEED", dto.SetSpeed);
                await WriteRegisterTag($"{dto.ServoCode}_JOG_SPEED", dto.JogSpeed);
                await WriteRegisterTag($"{dto.ServoCode}_SET", dto.SetValue);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"PLC write failed: {ex.Message}");
            }
        }

        /* ============================================================
           READ CURRENT SET VALUES — ✅ FIXED for ADS
        ============================================================ */
        [HttpGet]
        public async Task<IActionResult> GetCurrentSetValues(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                return BadRequest("ServoCode missing");

            try
            {
                int rawPos = await ReadRegisterTag($"{servoCode}_SET_POS");
                int rawSpeed = await ReadRegisterTag($"{servoCode}_SET_SPEED");
                int rawJog = await ReadRegisterTag($"{servoCode}_JOG_SPEED");
                int rawSet = await ReadRegisterTag($"{servoCode}_SET");

                return Ok(new
                {
                    setPosition = rawPos,
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
           HELPER: Write/Read register using symbolPath or address
        ============================================================ */
        private async Task WriteRegisterTag(string tagKey, int value)
        {
            var tag = GetTagOrThrow(tagKey);

            if (_plc.SupportsSymbolPath && !string.IsNullOrWhiteSpace(tag.SymbolPath))
            {
                await _plc.WriteIntAsync(tag.SymbolPath, (short)value);
            }
            else
            {
                await _plc.WriteHoldingRegisterAsync(tag.Address, value);
            }
        }

        private async Task<int> ReadRegisterTag(string tagKey)
        {
            var tag = GetTagOrThrow(tagKey);

            if (_plc.SupportsSymbolPath && !string.IsNullOrWhiteSpace(tag.SymbolPath))
            {
                return await _plc.ReadIntAsync(tag.SymbolPath);
            }
            else
            {
                return await _plc.ReadHoldingRegisterAsync(tag.Address);
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

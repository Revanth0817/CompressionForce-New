using CompressionForce.Integrations.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.Controllers
{
    [ApiController]
    [Route("api/modbus")]
    public class ModbusController : ControllerBase
    {
        private readonly ModbusService _modbus;

        public ModbusController(ModbusService modbus)
        {
            _modbus = modbus;
        }

        // GET: api/modbus/read?start=0&count=5
        [HttpGet("read")]
        public IActionResult ReadHoldingRegisters(int start = 0, int count = 5)
        {
            try
            {
                _modbus.Connect("127.0.0.1"); // PLC / Simulator IP
                var data = _modbus.ReadHoldingRegisters(start, count);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        // POST: api/modbus/write?address=1&value=100
        [HttpPost("write")]
        public IActionResult WriteSingleRegister(int address, int value)
        {
            try
            {
                _modbus.Connect("127.0.0.1");
                _modbus.WriteSingleRegister(address, value);
                return Ok("Write successful");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }
    }
}

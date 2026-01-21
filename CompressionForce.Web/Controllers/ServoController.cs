using CompressionForce.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompressionForce.Web.Controllers
{
    [Route("Servo")]
    public class ServoController : Controller
    {
        private readonly PlcService _plc;

        public ServoController(PlcService plc)
        {
            _plc = plc;
        }

        [HttpPost("Run")]
        public IActionResult Run(string servoCode)
        {
            _plc.Run(servoCode);
            return Ok();
        }

        [HttpPost("Stop")]
        public IActionResult Stop(string servoCode)
        {
            _plc.Stop(servoCode);
            return Ok();
        }

        [HttpPost("JogUp")]
        public IActionResult JogUp(string servoCode)
        {
            _plc.JogUp(servoCode);
            return Ok();
        }

        [HttpPost("JogDown")]
        public IActionResult JogDown(string servoCode)
        {
            _plc.JogDown(servoCode);
            return Ok();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Plc
{
    public class ServoMap
    {
        // WRITE (Calibration)
        public int JogSpeed { get; set; }
        public int TorqueLimit { get; set; }
        public int SetPosition { get; set; }
        public int SetSpeed { get; set; }

        // READ (Live feedback)
        public int ActTorque { get; set; }
        public int Ready { get; set; }
        public int Alarm { get; set; }
        public int JogUp { get; set; }
        public int JogDown { get; set; }
        public int Run { get; set; }
        public int Stop { get; set; }
    }
}

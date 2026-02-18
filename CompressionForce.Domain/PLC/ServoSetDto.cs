using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CompressionForce.Domain.PLC
{
    public class ServoSetDto
    {
        public string ServoCode { get; set; }
        public int SetValue { get; set; }
        public int SetPosition { get; set; }
        public int SetSpeed { get; set; }
        public int JogSpeed { get; set; }
    }

}




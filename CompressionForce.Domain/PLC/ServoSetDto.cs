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
        public double SetValue { get; set; }     
        public double SetPosition { get; set; }
        public double SetSpeed { get; set; }
        public double JogSpeed { get; set; }
    }

}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CompressionForce.Domain.Calibration
{
    public class SaveCalibrationDto
    {
        public string LoadCellName { get; set; }

        public double MinVolt { get; set; }
        public double MaxVolt { get; set; }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public double Factor { get; set; }
        public double Offset { get; set; }
    }
}

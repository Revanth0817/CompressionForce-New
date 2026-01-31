using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.Calibration
{
    public sealed class NoLoadOffset
    {
        public decimal Value { get; }

        public NoLoadOffset(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Offset cannot be negative");

            Value = value;
        }
    }
}

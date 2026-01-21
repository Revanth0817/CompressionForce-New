using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using MathNet.Numerics;

namespace CompressionForce.Domain.Calibration;

public class CalibrationCurve
{
    public double Slope { get; private set; }
    public double Intercept { get; private set; }

    public void Fit(double[] xValues, double[] yValues)
    {
        if (xValues.Length != yValues.Length)
            throw new ArgumentException("X and Y length mismatch");

        (Slope, Intercept) =
            MathNet.Numerics.Fit.Line(xValues, yValues);
    }

    public double CalculateConcentration(double response)
    {
        if (Slope == 0)
            throw new InvalidOperationException("Calibration not initialized");

        return (response - Intercept) / Slope;
    }
}

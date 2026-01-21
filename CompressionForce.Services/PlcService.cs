using CompressionForce.Domain.Calibration;
using CompressionForce.Domain.Entities;
using CompressionForce.Domain.Plc;
using System;
using System.Collections.Generic;

namespace CompressionForce.Services
{
    public class PlcService
    {
        private const double MAX_RAW = 32767.0;
        private const double MAX_VOLT = 10.0;

        private readonly PlcModbusClient _plc;
        private readonly PlcMapping _mapping;
        private readonly CalibrationRuntimeService _runtime;

        public PlcService(
            PlcModbusClient plc,
            PlcMappingProvider mappingProvider,
            CalibrationRuntimeService runtime)
        {
            _plc = plc ?? throw new ArgumentNullException(nameof(plc));
            _mapping = mappingProvider?.Mapping
                       ?? throw new ArgumentNullException(nameof(mappingProvider));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        // =========================
        // RAW ANALOG INPUT
        // =========================
        public int ReadAnalogRaw(string loadCellName)
        {
            if (string.IsNullOrWhiteSpace(loadCellName))
                throw new ArgumentException("Loadcell name is empty");

            if (!_mapping.AnalogInputs.TryGetValue(loadCellName, out int address))
                throw new Exception($"Analog input not mapped: {loadCellName}");

            var values = _plc.ReadInputRegisters(address, 1);

            if (values == null || values.Length == 0)
                throw new Exception($"PLC returned no data for {loadCellName}");

            return values[0];
        }

        // =========================
        // RAW → VOLT (0–10V)
        // =========================
        public double ReadAnalogVoltage(string loadCellName)
        {
            int raw = ReadAnalogRaw(loadCellName);
            double voltage = (raw / MAX_RAW) * MAX_VOLT;
            return Math.Round(voltage, 3);
        }

        // =========================
        // RAW → VOLT → kN (RUNTIME CALIBRATION)
        // =========================
        public (double Voltage, double Kn) ReadAnalogCalibrated(string loadCellName)
        {
            double voltage = ReadAnalogVoltage(loadCellName);

            // 🔥 Apply LIVE calibration (from Calibration page)
            if (_runtime.TryGet(loadCellName, out var cal))
            {
                double kn = (voltage * cal.Factor) + cal.Offset;

                return (
                    Math.Round(voltage, 3),
                    Math.Round(kn, 3)
                );
            }

            // 🔹 No calibration yet
            return (
                Math.Round(voltage, 3),
                0.0
            );
        }

        // =========================
        // ALL ANALOG INPUTS (DIAGNOSTICS)
        // =========================
        public IEnumerable<(string Key, double Voltage, double Kn)> GetAllAnalogInputs()
        {
            foreach (var key in _mapping.AnalogInputs.Keys)
            {
                var result = ReadAnalogCalibrated(key);
                yield return (key, result.Voltage, result.Kn);
            }
        }
        public void ApplyServoCalibration(string servoCode, ServoCalibration cal)
        {
            var map = _mapping.Servos[servoCode];

            _plc.WriteSingleRegister(map.JogSpeed, (int)cal.JogSpeed);
            _plc.WriteSingleRegister(map.TorqueLimit, (int)(cal.TorqueLimit * 10));
            _plc.WriteSingleRegister(map.SetPosition, (int)(cal.SetPosition * 100));
            _plc.WriteSingleRegister(map.SetSpeed, (int)cal.SetSpeed);

            Console.WriteLine($"Servo {servoCode} calibration applied to PLC");
        }
        public decimal ReadServoActTorque(string servoCode)
        {
            if (string.IsNullOrWhiteSpace(servoCode))
                throw new ArgumentException("servoCode missing");

            if (!_mapping.Servos.TryGetValue(servoCode, out var map))
                throw new Exception($"Servo mapping not found: {servoCode}");

            int raw = _plc.ReadHoldingRegisters(map.ActTorque, 1)[0];

            return raw / 10m; // scaling
        }


        public (bool Ready, bool Alarm) ReadServoStatus(string servoCode)
        {
            if (!_mapping.Servos.TryGetValue(servoCode, out var map))
                return (false, true); // fail-safe

            try
            {
                bool ready = _plc.ReadDiscreteInputs(map.Ready, 1)[0];
                bool alarm = _plc.ReadDiscreteInputs(map.Alarm, 1)[0];

                return (ready, alarm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Servo status read failed [{servoCode}]: {ex.Message}");
                return (false, true);
            }
        }




        public void JogUp(string servoCode)
        {
            var map = _mapping.Servos[servoCode];
            _plc.WriteSingleCoil(map.JogUp, true);
        }

        public void JogDown(string servoCode)
        {
            var map = _mapping.Servos[servoCode];
            _plc.WriteSingleCoil(map.JogDown, true);
        }

        public void Run(string servoCode)
        {
            var map = _mapping.Servos[servoCode];
            _plc.WriteSingleCoil(map.Run, true);
        }

        public void Stop(string servoCode)
        {
            var map = _mapping.Servos[servoCode];
            _plc.WriteSingleCoil(map.Stop, true);
        }

    }
}

using CompressionForce.Domain.Calibration;
using CompressionForce.Domain.PLC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompressionForce.Services
{
    public class PlcPollingBackgroundService : BackgroundService
    {
        private readonly IPlcProtocol _plc;
        private readonly PlcMemoryCache _cache;
        private readonly PlcTagConfig _config;
        private readonly IServoStatusPublisher _publisher;
        private readonly IServiceScopeFactory _scopeFactory;

        public PlcPollingBackgroundService(
            IPlcProtocol plc,
            PlcMemoryCache cache,
            PlcTagConfig config,
            IServoStatusPublisher publisher,
            IServiceScopeFactory scopeFactory)
        {
            _plc = plc;
            _cache = cache;
            _config = config;
            _publisher = publisher;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stop)
        {
            Console.WriteLine("✅ PLC POLLING SERVICE STARTED");

            await _plc.ConnectAsync();

            var groups = _config.Tags.GroupBy(t => t.Polling);

            foreach (var group in groups)
            {
                _ = Task.Run(
                    () => PollGroup(group.ToList(), stop),
                    stop
                );
            }
        }

        private async Task PollGroup(
            List<PlcTag> tags,
            CancellationToken stop)
        {
            while (!stop.IsCancellationRequested)
            {
                foreach (var tag in tags)
                {
                    try
                    {
                        object rawValue = tag.Type switch
                        {
                            PlcDataType.DiscreteInput =>
                                await _plc.ReadDiscreteInputAsync(tag.Address),

                            PlcDataType.Coil =>
                                await _plc.ReadCoilAsync(tag.Address),

                            PlcDataType.InputRegister =>
                                await _plc.ReadInputRegisterAsync(tag.Address),

                            PlcDataType.HoldingRegister =>
                                await _plc.ReadHoldingRegisterAsync(tag.Address),

                            _ => null!
                        };

                        /* ================= ANALOG INPUT ================= */
                        if (tag.Type == PlcDataType.InputRegister &&
                            tag.Key.StartsWith("LC_") &&
                            !string.IsNullOrWhiteSpace(tag.LoadCellCode))
                        {
                            int raw = (int)rawValue;
                            double voltage = ConvertInputRegisterToVoltage(raw);

                            // ✅ CREATE SCOPE FOR DB ACCESS
                            using var scope = _scopeFactory.CreateScope();
                            var repo = scope.ServiceProvider
                                .GetRequiredService<ICalibrationRepository>();

                            var cal = repo.GetLatest(tag.LoadCellCode);

                            double factor = cal != null ? (double)cal.Factor : 1.0;
                            double offset = cal != null ? (double)cal.Offset : 0.0;

                            double force = Math.Round(
                                (voltage * factor) + offset, 3
                            );

                            _cache.Set($"{tag.LoadCellCode}_RAW", raw);
                            _cache.Set($"{tag.LoadCellCode}_VOLT", voltage);
                            _cache.Set($"{tag.LoadCellCode}_KN", force);

                            await _publisher.PublishAnalogInputAsync(
                                tag.LoadCellCode,
                                voltage,
                                force
                            );

                            continue;
                        }

                        /* ================= DIGITAL INPUT ================= */
                        if (tag.Type == PlcDataType.DiscreteInput)
                        {
                            _cache.Set(tag.Key, rawValue);
                            await _publisher.PublishDigitalInputAsync(
                                tag.Key,
                                (bool)rawValue
                            );
                        }

                        /* ================= DIGITAL OUTPUT ================= */
                        if (tag.Type == PlcDataType.Coil)
                        {
                            _cache.Set(tag.Key, rawValue);
                            await _publisher.PublishDigitalOutputAsync(
                                tag.Key,
                                (bool)rawValue
                            );
                        }
                        /* ================= COUNTERS (INPUT REGISTERS) ================= */
                        if (tag.Type == PlcDataType.InputRegister &&
                            (tag.Key == "REVOLUTION_COUNT" ||
                             tag.Key == "ENCODER_ACT_COUNT"))
                        {
                            int value = Convert.ToInt32(rawValue);

                            _cache.Set(tag.Key, value);

                            await _publisher.PublishAnalogInputAsync(
                                tag.Key,     // reuse channel
                                value,       // voltage parameter reused
                                value        // force parameter reused
                            );

                            continue;
                        }


                        /* ================= SERVO STATUS ================= */
                        if (IsServoStatusTag(tag.Key))
                        {
                            _cache.Set(tag.Key, rawValue);
                            await PublishServoStatusFromCache(tag.Key);
                        }


                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"❌ PLC READ FAILED [{tag.Key}] : {ex.Message}"
                        );
                    }
                }

                await Task.Delay(200, stop);
            }
        }

        /* ================= SERVO ================= */
        private static bool IsServoStatusTag(string key)
        {
            return key.EndsWith("_ACT_TORQUE")
                || key.EndsWith("_ACT_POS")
                || key.EndsWith("_READY")
                || key.EndsWith("_ALARM");
        }

        private async Task PublishServoStatusFromCache(string tagKey)
        {
            var servoCode = tagKey
                .Replace("_ACT_TORQUE", "")
                .Replace("_ACT_POS", "")
                .Replace("_READY", "")
                .Replace("_ALARM", "");

            int torque = Convert.ToInt32(
                _cache.Get($"{servoCode}_ACT_TORQUE") ?? 0
            );

            int actPos = Convert.ToInt32(
                _cache.Get($"{servoCode}_ACT_POS") ?? 0
            );

            bool ready = (_cache.Get($"{servoCode}_READY") as bool?) ?? false;
            bool alarm = (_cache.Get($"{servoCode}_ALARM") as bool?) ?? false;

            await _publisher.PublishServoAsync(
                servoCode,
                ready,
                alarm,
                torque,
                actPos
            );
        }


        private static double ConvertInputRegisterToVoltage(int raw)
        {
            return Math.Round(raw * 10.0 / 32767.0, 3);
        }
    }
}

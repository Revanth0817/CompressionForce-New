using CompressionForce.Data;
using CompressionForce.Data.Entities;
using CompressionForce.Domain.Calibration;
using CompressionForce.Domain.PLC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
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

        // ✅ Cache calibration to avoid DB query every poll cycle
        private readonly ConcurrentDictionary<string, (double Factor, double Offset)> _calCache = new();

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

            // ✅ Retry until connected
            while (!stop.IsCancellationRequested)
            {
                try
                {
                    await _plc.ConnectAsync();
                    Console.WriteLine("✅ PLC connected!");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ PLC connect failed: {ex.Message} — retrying in 5s...");
                    await Task.Delay(5000, stop);
                }
            }

            // ✅ Pre-load calibration data once
            LoadCalibrationCache();

            var groups = _config.Tags.GroupBy(t => t.Polling);

            foreach (var group in groups)
            {
                var pollingKey = group.Key;

                // ✅ Use configured interval, fallback to 200ms
                int intervalMs = _config.PollingIntervals.TryGetValue(pollingKey, out var ms)
                    ? ms
                    : 200;

                _ = Task.Run(
                    () => PollGroup(group.ToList(), intervalMs, stop),
                    stop
                );
            }

            // ✅ Refresh calibration cache every 30s (not every read)
            _ = Task.Run(async () =>
            {
                while (!stop.IsCancellationRequested)
                {
                    await Task.Delay(30_000, stop);
                    LoadCalibrationCache();
                }
            }, stop);

            // ✅ Heartbeat loop
            _ = Task.Run(async () =>
            {
                while (!stop.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider
                            .GetRequiredService<ApplicationDbContext>();

                        var status = await db.PlcStatuses.FirstOrDefaultAsync(stop);

                        if (status == null)
                        {
                            status = new PlcStatus();
                            db.PlcStatuses.Add(status);
                        }

                        status.IsPlcConnected = _plc.IsConnected;
                        status.IsLocalDbConnected = true;
                        status.PlcHeartbeat = DateTime.Now;
                        status.LastUpdated = DateTime.Now;
                        status.PlcIp = _plc.Host ?? "";

                        await db.SaveChangesAsync(stop);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Heartbeat error: {ex.Message}");
                    }

                    await Task.Delay(3000, stop);
                }
            }, stop);
        }

        private void LoadCalibrationCache()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider
                    .GetRequiredService<ICalibrationRepository>();

                var loadCellTags = _config.Tags
                    .Where(t => !string.IsNullOrWhiteSpace(t.LoadCellCode))
                    .Select(t => t.LoadCellCode!)
                    .Distinct();

                foreach (var code in loadCellTags)
                {
                    var cal = repo.GetLatest(code);
                    double factor = cal != null ? (double)cal.Factor : 1.0;
                    double offset = cal != null ? (double)cal.Offset : 0.0;
                    _calCache[code] = (factor, offset);
                }

                Console.WriteLine($"✅ Calibration cache loaded ({_calCache.Count} sensors)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calibration cache error: {ex.Message}");
            }
        }

        private async Task PollGroup(
            List<PlcTag> tags,
            int intervalMs,
            CancellationToken stop)
        {
            var useSymbol = _plc.SupportsSymbolPath;

            while (!stop.IsCancellationRequested)
            {
                foreach (var tag in tags)
                {
                    try
                    {
                        object rawValue;

                        // ✅ Prefer symbolPath for ADS, fallback to address for Modbus
                        if (useSymbol && !string.IsNullOrWhiteSpace(tag.SymbolPath))
                        {
                            rawValue = tag.Type switch
                            {
                                PlcDataType.DiscreteInput =>
                                    await _plc.ReadBoolAsync(tag.SymbolPath),

                                PlcDataType.Coil =>
                                    await _plc.ReadBoolAsync(tag.SymbolPath),

                                PlcDataType.InputRegister =>
                                    (int)await _plc.ReadIntAsync(tag.SymbolPath),

                                PlcDataType.HoldingRegister =>
                                    (int)await _plc.ReadIntAsync(tag.SymbolPath),

                                PlcDataType.RealInput =>
                                    await _plc.ReadFloatAsync(tag.SymbolPath),

                                _ => null!
                            };
                        }
                        else
                        {
                            rawValue = tag.Type switch
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
                        }

                        /* ================= ANALOG INPUT ================= */
                        if (tag.Type == PlcDataType.InputRegister &&
                            tag.Key.StartsWith("LC_") &&
                            !string.IsNullOrWhiteSpace(tag.LoadCellCode))
                        {
                            double raw = Convert.ToDouble(rawValue);

                            var (factor, offset) = _calCache.TryGetValue(tag.LoadCellCode, out var cal)
                                ? cal
                                : (1.0, 0.0);

                            double force = Math.Round(
                                (raw * factor) + offset, 3
                            );

                            _cache.Set($"{tag.LoadCellCode}_RAW", raw);
                            _cache.Set($"{tag.LoadCellCode}_KN", force);

                            await _publisher.PublishAnalogInputAsync(
                                tag.LoadCellCode,
                                raw,
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

                            continue;
                        }

                        /* ================= DIGITAL OUTPUT ================= */
                        if (tag.Type == PlcDataType.Coil)
                        {
                            _cache.Set(tag.Key, rawValue);
                            await _publisher.PublishDigitalOutputAsync(
                                tag.Key,
                                (bool)rawValue
                            );

                            continue;
                        }

                        /* ================= COUNTERS (INPUT REGISTERS) ================= */
                        if (tag.Type == PlcDataType.InputRegister &&
                            (tag.Key == "REVOLUTION_COUNT" ||
                             tag.Key == "ENCODER_ACT_COUNT"))
                        {
                            int value = Convert.ToInt32(rawValue);

                            _cache.Set(tag.Key, value);

                            await _publisher.PublishAnalogInputAsync(
                                tag.Key,
                                value,
                                value
                            );

                            continue;
                        }

                        /* ================= SERVO STATUS ================= */
                        if (IsServoStatusTag(tag.Key))
                        {
                            _cache.Set(tag.Key, rawValue);
                            await PublishServoStatusFromCache(tag.Key);
                            continue;
                        }

                        /* ================= DEFAULT: cache & publish tag ================= */
                        if (tag.Type == PlcDataType.InputRegister ||
                            tag.Type == PlcDataType.HoldingRegister)
                        {
                            _cache.Set(tag.Key, rawValue);
                            await _publisher.PublishTagAsync(tag.Key, rawValue);
                        }

                        /* ================= REAL INPUT (GRAPH) ================= */
                        if (tag.Type == PlcDataType.RealInput)
                        {
                            float value = Convert.ToSingle(rawValue);

                            _cache.Set(tag.Key, value);

                            await _publisher.PublishAnalogInputAsync(
                                tag.Key,
                                value,
                                value
                            );

                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"❌ PLC READ FAILED [{tag.Key}] : {ex.Message}"
                        );
                    }
                }

                // ✅ Use the configured polling interval
                if (intervalMs > 0)
                    await Task.Delay(intervalMs, stop);
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
    }
}

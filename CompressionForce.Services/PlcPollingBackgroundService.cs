using CompressionForce.Domain.PLC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompressionForce.Services;

public class PlcPollingBackgroundService : BackgroundService
{
    private readonly IPlcProtocol _plc;
    private readonly PlcMemoryCache _cache;
    private readonly PlcTagConfig _config;
    private readonly IServoStatusPublisher _publisher;

    public PlcPollingBackgroundService(
        IPlcProtocol plc,
        PlcMemoryCache cache,
        IConfiguration cfg,
        IServoStatusPublisher publisher)
    {
        _plc = plc;
        _cache = cache;
        _publisher = publisher;
        _config = PlcConfigLoader.Load(cfg["Plc:TagsFile"]!);
    }

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        Console.WriteLine("✅ PLC POLLING SERVICE STARTED");

        await _plc.ConnectAsync();

        var groups = _config.Tags.GroupBy(t => t.Polling);

        foreach (var group in groups)
        {
            _ = Task.Run(
                () => PollGroup(group.Key, group.ToList(), stop),
                stop
            );
        }
    }

    private async Task PollGroup(
        string pollingKey,
        List<PlcTag> tags,
        CancellationToken stop)
    {
        var delay = _config.PollingIntervals[pollingKey];

        Console.WriteLine($"🔄 Polling '{pollingKey}' every {delay} ms");

        while (!stop.IsCancellationRequested)
        {
            foreach (var tag in tags)
            {
                try
                {
                    object value = tag.Type switch
                    {
                        /* ================= DIGITAL INPUT ================= */
                        PlcDataType.DiscreteInput =>
                            await _plc.ReadDiscreteInputAsync(tag.Address),

                        /* ================= DIGITAL OUTPUT ================= */
                        PlcDataType.Coil =>
                            await _plc.ReadCoilAsync(tag.Address),

                        /* ================= ANALOG INPUT ================= */
                        PlcDataType.InputRegister =>
                            tag.Key.StartsWith("LC_")
                                ? ConvertInputRegisterToVoltage(
                                      await _plc.ReadInputRegisterAsync(tag.Address))
                                : await _plc.ReadInputRegisterAsync(tag.Address),

                        /* ================= HOLDING REGISTER ================= */
                        PlcDataType.HoldingRegister =>
                            await _plc.ReadHoldingRegisterAsync(tag.Address),

                        _ => null!
                    };

                    if (string.IsNullOrWhiteSpace(tag.Key))
                        continue;

                    /* CACHE ALWAYS UPDATED */
                    _cache.Set(tag.Key, value);

                    /* =====================================================
                       DIGITAL INPUT → UI
                    ===================================================== */
                    if (tag.Type == PlcDataType.DiscreteInput)
                    {
                        await _publisher.PublishDigitalInputAsync(
                            tag.Key,
                            (bool)value
                        );
                    }

                    /* =====================================================
                       DIGITAL OUTPUT (COIL) → UI
                    ===================================================== */
                    if (tag.Type == PlcDataType.Coil)
                    {
                        await _publisher.PublishDigitalOutputAsync(
                            tag.Key,
                            (bool)value
                        );
                    }

                    /* =====================================================
                       SERVO STATUS (AGGREGATED)
                    ===================================================== */
                    if (IsServoStatusTag(tag.Key))
                    {
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

            await Task.Delay(delay, stop);
        }
    }

    /* ============================================================
       SERVO STATUS AGGREGATION
    ============================================================ */

    private static bool IsServoStatusTag(string key)
    {
        return key.EndsWith("_ACT_TORQUE")
            || key.EndsWith("_READY")
            || key.EndsWith("_ALARM");
    }

    private async Task PublishServoStatusFromCache(string tagKey)
    {
        var servoCode = tagKey
            .Replace("_ACT_TORQUE", "")
            .Replace("_READY", "")
            .Replace("_ALARM", "");

        int torque = Convert.ToInt32(
            _cache.Get($"{servoCode}_ACT_TORQUE") ?? 0
        );

        bool ready = (_cache.Get($"{servoCode}_READY") as bool?) ?? false;
        bool alarm = (_cache.Get($"{servoCode}_ALARM") as bool?) ?? false;

        await _publisher.PublishServoAsync(
            servoCode,
            ready,
            alarm,
            torque
        );
    }

    /* ============================================================
       RAW ADC → VOLTAGE (LOADCELL)
    ============================================================ */
    private static double ConvertInputRegisterToVoltage(int raw)
    {
        const double MaxVoltage = 10.0;
        const double MaxAdc = 32767.0;

        return Math.Round(raw * MaxVoltage / MaxAdc, 3);
    }
}

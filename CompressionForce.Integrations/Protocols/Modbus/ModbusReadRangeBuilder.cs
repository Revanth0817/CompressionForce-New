using System;
using System.Collections.Generic;
using System.Linq;
using CompressionForce.Domain.Entities;

namespace CompressionForce.Integrations.Protocols.Modbus
{
    public static class ModbusReadRangeBuilder
    {
        public static IReadOnlyList<ModbusReadRange> Build(
            IEnumerable<PlcSignal> signals)
        {
            var modbusSignals = signals
                .Where(s => s.Address != null &&
                            string.Equals(
                                s.Address.Kind,
                                "Numeric",
                                StringComparison.OrdinalIgnoreCase))
                .ToList();

            return modbusSignals
                .GroupBy(s => ParseArea(s.Address!.Area))
                .SelectMany(BuildRangesForArea)
                .ToList();
        }

        // ------------------------------------------------------------
        // Build contiguous ranges per Modbus area
        // ------------------------------------------------------------
        private static IEnumerable<ModbusReadRange> BuildRangesForArea(
            IGrouping<ModbusRegisterType, PlcSignal> group)
        {
            var ordered = group
                .Select(s => new
                {
                    Signal = s,
                    Start = ushort.Parse(s.Address!.Value),
                    Length = (ushort)s.Address.Length
                })
                .OrderBy(x => x.Start)
                .ToList();

            ushort currentStart = 0;
            ushort currentLength = 0;
            var currentSignals = new List<PlcSignal>();
            bool hasActiveRange = false;

            foreach (var item in ordered)
            {
                if (!hasActiveRange)
                {
                    currentStart = item.Start;
                    currentLength = item.Length;
                    currentSignals.Add(item.Signal);
                    hasActiveRange = true;
                    continue;
                }

                // Contiguous?
                if (item.Start == currentStart + currentLength)
                {
                    currentLength += item.Length;
                    currentSignals.Add(item.Signal);
                }
                else
                {
                    yield return new ModbusReadRange(
                        group.Key,
                        currentStart,
                        currentLength,
                        currentSignals.ToList());

                    currentStart = item.Start;
                    currentLength = item.Length;
                    currentSignals.Clear();
                    currentSignals.Add(item.Signal);
                }
            }

            if (hasActiveRange)
            {
                yield return new ModbusReadRange(
                    group.Key,
                    currentStart,
                    currentLength,
                    currentSignals.ToList());
            }
        }

        // ------------------------------------------------------------
        // Map string → ModbusRegisterType
        // ------------------------------------------------------------
        private static ModbusRegisterType ParseArea(string area)
        {
            if (!Enum.TryParse<ModbusRegisterType>(area, true, out var result))
                throw new InvalidOperationException(
                    $"Unsupported Modbus area '{area}'");

            return result;
        }
    }
}

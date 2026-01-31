using CompressionForce.Services.DTOs;
using CompressionForce.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Services.AutoTare
{
    public sealed class AutoTareQueryService
    {
        private readonly PlcSignalCache _cache;

        public AutoTareQueryService(PlcSignalCache cache)
        {
            _cache = cache;
        }

        public AutoTareDto GetSnapshot()
        {
            return new AutoTareDto
            {
                MotorStatus = GetBool("MOTOR_STATUS"),
                MotorTrip = GetBool("MOTOR_TRIP"),
                Revolutions = GetInt("REVOLUTIONS"),

                S1Main = GetDec("LC_S1_MAIN"),
                S2Main = GetDec("LC_S2_MAIN"),
                S1Pre = GetDec("LC_S1_PRE"),
                S2Pre = GetDec("LC_S2_PRE"),
                S1Eject = GetDec("LC_S1_EJECT"),
                S2Eject = GetDec("LC_S2_EJECT")
            };
        }

        private bool GetBool(string key)
        {
            return _cache.TryGet(key, out var value)
                   && value is bool b
                   && b;
        }

        private int GetInt(string key)
            => _cache.TryGet(key, out var v) ? Convert.ToInt32(v) : 0;

        private decimal GetDec(string key)
            => _cache.TryGet(key, out var v) ? Convert.ToDecimal(v) : 0m;
    }
}

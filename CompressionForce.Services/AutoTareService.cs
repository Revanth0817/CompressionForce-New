using CompressionForce.Services.DTOs;
using CompressionForce.Services.Interfaces;
using CompressionForce.Services.Plc;

namespace CompressionForce.Services
{


    public sealed class AutoTareService : IAutoTareService
    {
        private readonly PlcSignalCache _cache;

        public AutoTareService(PlcSignalCache cache)
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

                S1Main = GetDecimal("LC_S1_MAIN"),
                S2Main = GetDecimal("LC_S2_MAIN"),
                S1Pre = GetDecimal("LC_S1_PRE"),
                S2Pre = GetDecimal("LC_S2_PRE"),
                S1Eject = GetDecimal("LC_S1_EJECT"),
                S2Eject = GetDecimal("LC_S2_EJECT")
            };
        }

        private bool GetBool(string key)
            => _cache.TryGet(key, out var v) && v is bool b && b;

        private int GetInt(string key)
            => _cache.TryGet(key, out var v) && v is int i ? i : 0;

        private decimal GetDecimal(string key)
            => _cache.TryGet(key, out var v) && v is decimal d ? d : 0m;
    }
}

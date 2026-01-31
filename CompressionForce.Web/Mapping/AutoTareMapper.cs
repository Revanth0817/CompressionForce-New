using CompressionForce.Services.DTOs;
using CompressionForce.Web.Models;

namespace CompressionForce.Web.Mapping
{
    public static class AutoTareMapper
    {
        public static AutoTareVm ToVm(this AutoTareDto dto)
        {
            return new AutoTareVm
            {
                MotorStatus = dto.MotorStatus ? "On" : "Off",
                MotorTrip = dto.MotorTrip ? "On" : "Off",
                Revolutions = dto.Revolutions,

                S1Main = dto.S1Main,
                S2Main = dto.S2Main,
                S1Pre = dto.S1Pre,
                S2Pre = dto.S2Pre,
                S1Eject = dto.S1Eject,
                S2Eject = dto.S2Eject
            };
        }
    }
}

using CompressionForce.Data;
using CompressionForce.Services.DTOs;
using System;
using System.Linq;

namespace CompressionForce.Services
{
    public class AutoTareService
    {
        private readonly ApplicationDbContext _db;

        public AutoTareService(ApplicationDbContext db)
        {
            _db = db;
        }

        public AutoTareVm GetStatus()
        {
            var row = _db.AutoTareStatuses
                .OrderByDescending(r => r.last_updated)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            if (row == null)
            {
                return new AutoTareVm(); // prevents NullReferenceException
            }

            return new AutoTareVm
            {
                HasData = true,
                LastUpdatedUtc = DateTime.SpecifyKind(row.last_updated, DateTimeKind.Utc),
                MotorStatus = row.MotorStatus ? "On" : "Off",
                MotorTrip = row.MotorTrip ? "On" : "Off",
                Revolutions = row.Revolutions,

                S1Main = row.S1Main,
                S2Main = row.S2Main,
                S1Pre = row.S1Pre,
                S2Pre = row.S2Pre,
                S1Eject = row.S1Eject,
                S2Eject = row.S2Eject
            };
        }
    }
}

using CompressionForce.Data;
using CompressionForce.Services.DTOs;
using CompressionForce.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace CompressionForce.Services
{
    public class PlcStatusService : IPlcStatusService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;

        public PlcStatusService(
            ApplicationDbContext db,
            IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        public PlcStatusVm GetPlcStatus()
        {
            var session = _http.HttpContext?.Session;

            // Session values
            var userName = session?.GetString("UserName") ?? "";
            var userLevel = session?.GetString("UserLevel") ?? "";
            var loginTimeStr = session?.GetString("LoginTime");

            var loginTimeIso = session?.GetString("LoginTime") ?? "";
            string loginOn = "";
            string appRunTime = "";

            if (DateTime.TryParse(loginTimeStr, out var loginTime))
            {
                loginOn = loginTime.ToString("hh:mm tt");

                var diff = DateTime.Now - loginTime;
                appRunTime = $"{(int)diff.TotalHours}H:{diff.Minutes}M";
            }

            // the local database is "connected" when this very query reaches it - not a flag somebody has to write
            Data.Entities.PlcStatus status = null;
            var dbReachable = true;
            try
            {
                status = _db.PlcStatuses
                    .OrderByDescending(x => x.LastUpdated)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                dbReachable = false;
            }

            if (status == null)
            {
                return new PlcStatusVm
                {
                    Date = DateTime.Now.ToString("dd MMM yyyy"),
                    Time = DateTime.Now.ToString("hh:mm tt"),
                    PlcConnected = false,
                    LocalDbConnected = dbReachable,

                    UserName = userName,
                    UserLevel = userLevel,
                    UserLoginOn = loginOn,
                    AppRunTime = appRunTime,
                    LoginTimeIso = loginTimeIso
                };
            }

            // timestamptz columns come back as UTC; compare in the same kind
            var heartbeatAge = status.PlcHeartbeat.Kind == DateTimeKind.Local
                ? DateTime.Now - status.PlcHeartbeat
                : DateTime.UtcNow - DateTime.SpecifyKind(status.PlcHeartbeat, DateTimeKind.Utc);
            var plcAlive = heartbeatAge.TotalSeconds <= 10;
            var lastUpdated = status.LastUpdated.Kind == DateTimeKind.Utc
                ? status.LastUpdated.ToLocalTime()
                : status.LastUpdated;

            return new PlcStatusVm
            {
                Date = lastUpdated.ToString("dd MMM yyyy"),
                Time = lastUpdated.ToString("hh:mm tt"),
                PlcIp = status.PlcIp,

                LocalDbConnected = dbReachable && status.IsLocalDbConnected,
                PlcConnected = plcAlive,

                UserName = userName,
                UserLevel = userLevel,
                UserLoginOn = loginOn,
                AppRunTime = appRunTime,
                LoginTimeIso = loginTimeIso
            };
        }
    }
}

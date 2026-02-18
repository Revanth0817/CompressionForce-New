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

            string loginOn = "";
            string appRunTime = "";

            if (DateTime.TryParse(loginTimeStr, out var loginTime))
            {
                loginOn = loginTime.ToString("hh:mm tt");

                var diff = DateTime.Now - loginTime;
                appRunTime = $"{diff.Hours}H:{diff.Minutes}M";
            }

            var status = _db.PlcStatuses
                .OrderByDescending(x => x.LastUpdated)
                .FirstOrDefault();

            if (status == null)
            {
                return new PlcStatusVm
                {
                    Date = DateTime.Now.ToString("dd MMM yyyy"),
                    Time = DateTime.Now.ToString("hh:mm tt"),
                    PlcConnected = false,
                    LocalDbConnected = false,

                    UserName = userName,
                    UserLevel = userLevel,
                    UserLoginOn = loginOn,
                    AppRunTime = appRunTime
                };
            }

            var plcAlive =
                (DateTime.Now - status.PlcHeartbeat).TotalSeconds <= 10;

            return new PlcStatusVm
            {
                Date = status.LastUpdated.ToString("dd MMM yyyy"),
                Time = status.LastUpdated.ToString("hh:mm tt"),
                PlcIp = status.PlcIp,

                LocalDbConnected = status.IsLocalDbConnected,
                PlcConnected = plcAlive,

                UserName = userName,
                UserLevel = userLevel,
                UserLoginOn = loginOn,
                AppRunTime = appRunTime
            };
        }
    }
}
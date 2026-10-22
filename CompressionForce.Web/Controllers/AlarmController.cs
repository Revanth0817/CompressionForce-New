using CompressionForce.Web.Authorization;
using CompressionForce.Data;
using CompressionForce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace CompressionForce.Controllers
{
    [RequirePrivilege("Alarm")]
    public class AlarmController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlarmController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // Alarm Page
        // ===============================
        public IActionResult Index()
        {
            // the alarm screen lives under Home/Alarm
            return RedirectToAction("Alarm", "Home");
        }

        // ===============================
        // AUTO FETCH – UI (JSON)
        // ===============================
        private const int MaxRows = 2000;   // the screen refreshes every few seconds: never send "everything"

        // the From / To boxes hold local times; a "To" at midnight means the whole day
        private static IQueryable<CompressionForce.Domain.Entities.AlarmLog> ApplyRange(
            IQueryable<CompressionForce.Domain.Entities.AlarmLog> query, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Local).ToUniversalTime();
                query = query.Where(a => a.AlarmCreatedTime >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.TimeOfDay == TimeSpan.Zero
                    ? toDate.Value.Date.AddDays(1).AddTicks(-1)
                    : toDate.Value;
                to = DateTime.SpecifyKind(to, DateTimeKind.Local).ToUniversalTime();
                query = query.Where(a => a.AlarmCreatedTime <= to);
            }

            return query;
        }

        [HttpGet]
        public IActionResult GetAllAlarms(DateTime? fromDate, DateTime? toDate)
        {
            var query = ApplyRange(_context.AlarmLogs.AsNoTracking(), fromDate, toDate);

            var total = query.Count();

            var alarms = query
                .OrderByDescending(a => a.AlarmCreatedTime)
                .Take(MaxRows)
                .ToList()
                .Select(a => new
                {
                    a.AlarmCode,
                    AlarmCreatedTime = a.AlarmCreatedTime,
                    AlarmCreatedTimeText = a.AlarmCreatedTime
                        .ToLocalTime()
                        .ToString("dd-MM-yyyy HH:mm:ss"),
                    a.AlarmDescription,
                    a.UserName
                })
                .ToList();

            return Json(new { total, truncated = total > alarms.Count, alarms });
        }

        // ===============================
        // PDF DOWNLOAD – ROTATIVA
        // ===============================
        [HttpGet]
        public IActionResult DownloadAlarmPdf(DateTime? fromDate, DateTime? toDate)
        {
            var query = ApplyRange(_context.AlarmLogs.AsNoTracking(), fromDate, toDate);

            var data = query
                .OrderByDescending(a => a.AlarmCreatedTime)
                .Select(a => new AlarmReportVm
                {
                    AlarmCode = a.AlarmCode,
                    AlarmCreatedTime = a.AlarmCreatedTime,
                    AlarmDescription = a.AlarmDescription,
                    UserName = a.UserName
                })
                .ToList();

            return new ViewAsPdf("AlarmPdf", data)
            {
                FileName = $"Alarm_Report_{DateTime.Now:ddMMyyyy_HHmm}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait, // same as AuditTrail
            
            };
        }
    }
}

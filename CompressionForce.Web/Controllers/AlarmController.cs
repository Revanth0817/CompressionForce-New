using CompressionForce.Data;
using CompressionForce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace CompressionForce.Controllers
{
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
            return View();
        }

        // ===============================
        // AUTO FETCH – UI (JSON)
        // ===============================
        [HttpGet]
        public IActionResult GetAllAlarms()
        {
            var alarms = _context.AlarmLogs
                .AsNoTracking()
                .OrderByDescending(a => a.AlarmCreatedTime)
                .Select(a => new
                {
                    a.AlarmCode,
                    AlarmCreatedTime = a.AlarmCreatedTime, // for JS filter
                    AlarmCreatedTimeText = a.AlarmCreatedTime
                        .ToString("dd-MM-yyyy HH:mm:ss"),
                    a.AlarmDescription,
                    a.UserName
                })
                .ToList();

            return Json(alarms);
        }

        // ===============================
        // PDF DOWNLOAD – ROTATIVA
        // ===============================
        [HttpGet]
        public IActionResult DownloadAlarmPdf(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AlarmLogs.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.AlarmCreatedTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.AlarmCreatedTime <= toDate.Value);
            }

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

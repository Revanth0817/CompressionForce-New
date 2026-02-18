using CompressionForce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace CompressionForce.Controllers
{
    public class AuditTrailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditTrailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // MAIN PAGE
        // ===============================
        public IActionResult AuditTrail(
            string userName,
            string activity,
            string fromDate,
            string toDate)
        {
            var query = _context.AuditTrails
                .AsNoTracking()
                .AsQueryable();

            // FROM DATE
            if (!string.IsNullOrWhiteSpace(fromDate) &&
                DateTime.TryParse(fromDate, out DateTime fromDt))
            {
                fromDt = DateTime.SpecifyKind(fromDt, DateTimeKind.Utc);
                query = query.Where(x => x.DateTime >= fromDt);
            }

            // TO DATE
            if (!string.IsNullOrWhiteSpace(toDate) &&
                DateTime.TryParse(toDate, out DateTime toDt))
            {
                var endOfDayUtc = DateTime.SpecifyKind(
                    toDt.Date.AddDays(1).AddSeconds(-1),
                    DateTimeKind.Utc
                );

                query = query.Where(x => x.DateTime <= endOfDayUtc);
            }

            // USER FILTER
            if (!string.IsNullOrWhiteSpace(userName) && userName != "None")
            {
                query = query.Where(x => x.UserName == userName);
            }

            // ACTIVITY FILTER
            if (!string.IsNullOrWhiteSpace(activity) && activity != "None")
            {
                query = query.Where(x => x.Activity == activity);
            }

            var auditList = query
                .OrderByDescending(x => x.DateTime)
                .ToList();

            // USER DROPDOWN
            ViewBag.Users = _context.UserManagements
                .AsNoTracking()
                .Where(u => u.IsActive)
                .Select(u => u.ERname)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // ACTIVITY DROPDOWN (DYNAMIC)
            ViewBag.Activities = _context.AuditTrails
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Activity))
                .Select(x => x.Activity)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.TotalCount = auditList.Count;

            return View(auditList);
        }

        // ===============================
        // PDF DOWNLOAD / PRINT
        // ===============================
        [HttpGet]
        public IActionResult DownloadPdf(
            string userName,
            string activity,
            string fromDate,
            string toDate,
            bool print = false)
        {
            var query = _context.AuditTrails
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fromDate) &&
                DateTime.TryParse(fromDate, out DateTime fromDt))
            {
                fromDt = DateTime.SpecifyKind(fromDt, DateTimeKind.Utc);
                query = query.Where(x => x.DateTime >= fromDt);
            }

            if (!string.IsNullOrWhiteSpace(toDate) &&
                DateTime.TryParse(toDate, out DateTime toDt))
            {
                var endOfDayUtc = DateTime.SpecifyKind(
                    toDt.Date.AddDays(1).AddSeconds(-1),
                    DateTimeKind.Utc
                );

                query = query.Where(x => x.DateTime <= endOfDayUtc);
            }

            if (!string.IsNullOrWhiteSpace(userName) && userName != "None")
            {
                query = query.Where(x => x.UserName == userName);
            }

            if (!string.IsNullOrWhiteSpace(activity) && activity != "None")
            {
                query = query.Where(x => x.Activity == activity);
            }

            var data = query
                .OrderByDescending(x => x.DateTime)
                .ToList();

            ViewBag.PrintMode = print;

            return new ViewAsPdf("AuditTrailPdf", data)
            {
                FileName = "AuditTrail_Report.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
    }
}

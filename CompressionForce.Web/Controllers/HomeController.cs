using CompressionForce.Web.Authorization;
using Microsoft.AspNetCore.Http;
using CompressionForce.Data;
using CompressionForce.Models;
using CompressionForce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using CompressionForce.Services.DTOs;


namespace CompressionForce.WebControllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly AutoTareService _autoTareService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            AutoTareService autoTareService)
        {
            _logger = logger;
            _context = context;
            _autoTareService = autoTareService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Welcome");
        }

        /* Diagnostics Page */
        [RequirePrivilege("Diagnostics")]
        public IActionResult Diagnostics()
        {
            return View();
        }

        /* Report Pages */
        [RequirePrivilege("Report")]
        public IActionResult Table()
        {
            // the report lives in ReportController (this screen was an unfinished copy)
            return RedirectToAction("Report", "Report");
        }

        [RequirePrivilege("Report")]
        public IActionResult Graph()
        {
            // the report lives in ReportController (this screen was an unfinished copy)
            return RedirectToAction("Report", "Report");
        }

        /* ===================== AUTO TARE ===================== */
        [RequirePrivilege("AutoTareMode")]
        public IActionResult AutoTare()
        {
            CompressionForce.Services.DTOs.AutoTareVm model =
                _autoTareService.GetStatus();

            return View(model);
        }
        [RequirePrivilege("HelpMenu")]
        public IActionResult HelpMenu()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        /// <summary>Latest auto-tare reading as JSON (the Auto Tare screen polls this).</summary>
        [RequirePrivilege("AutoTareMode")]
        [HttpGet]
        public IActionResult AutoTareStatus()
        {
            var s = _autoTareService.GetStatus();
            return Json(new
            {
                hasData = s.HasData,
                lastUpdated = s.LastUpdatedUtc,
                ageSeconds = s.LastUpdatedUtc.HasValue
                    ? Math.Max(0, (DateTime.UtcNow - s.LastUpdatedUtc.Value).TotalSeconds)
                    : (double?)null,
                motorStatus = s.MotorStatus,
                motorTrip = s.MotorTrip,
                revolutions = s.Revolutions,
                s1Main = s.S1Main, s2Main = s.S2Main,
                s1Pre = s.S1Pre, s2Pre = s.S2Pre,
                s1Eject = s.S1Eject, s2Eject = s.S2Eject
            });
        }

        // no privilege needed: this is where failures end up (see Program.cs)
        [Microsoft.AspNetCore.Mvc.ResponseCache(Duration = 0, Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            var status = code is >= 400 and <= 599 ? code.Value : 500;
            Response.StatusCode = status;

            return View(new CompressionForce.Web.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = status
            });
        }

        public IActionResult Welcome()
        {
            return View();
        }

        /* Access Management */
        [RequirePrivilege("AccessManagement")]
        public IActionResult AccessManagement()
        {
            return View();
        }

        /* Alarm page */
        [RequirePrivilege("Alarm")]
        public IActionResult Alarm()
        {
            return View();
        }

        /* ===================== AUDIT TRAIL ===================== */
        [RequirePrivilege("AuditTrail")]
        public IActionResult AuditTrail()
        {
            // the audit trail screen lives in AuditTrailController (this action had no view)
            return RedirectToAction("AuditTrail", "AuditTrail");
        }

        /* Auto Mode pages */
        [RequirePrivilege("AutoMode")]
        public IActionResult AutoMode()
        {
            return View();
        }

        [RequirePrivilege("AutoMode")]
        public IActionResult OperationMode()
        {
            return View();
        }

        /* Batch pages */
        [RequirePrivilege("Batch")]
        public IActionResult Batch()
        {
            return View();
        }

        [RequirePrivilege("Calibration")]
        public IActionResult Calibration()
        {
            return View();
        }

        public IActionResult ViewBatch()
        {
            // batch details are shown on the Batch screen
            return RedirectToAction("Batch");
        }

        /* Manual Mode */
        [RequirePrivilege("AutoMode")]
        public IActionResult ManualMode()
        {
            return View();
        }

        /* Signal Page */
        [RequirePrivilege("Diagnostics")]
        public IActionResult Signal()
        {
            return View();
        }
    }
}

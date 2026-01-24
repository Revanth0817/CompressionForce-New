using CompressionForce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.WebControllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /* Diagnostics Page */
        public IActionResult Diagnostics()
        {
            return View();
        }

        /* Report Pages */
        public IActionResult Table()
        {
            return View();
        }

        public IActionResult Graph()
        {
            return View();
        }

        /* Auto Tare */
        public IActionResult AutoTare()
        {
            return View();
        }

        public IActionResult Welcome()
        {
            return View();
        }

        /* Access Management */
        public IActionResult AccessManagement()
        {
            return View();
        }

        /* Alarm page */
        public IActionResult Alarm()
        {
            return View();
        }

        /* ===================== AUDIT TRAIL ===================== */
        public async Task<IActionResult> AuditTrail()
        {
            // Dropdown users
            ViewBag.Users = await _context.UserManagements
                .Where(u => u.IsActive)
                .Select(u => u.ERname)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            // AuditTrail table data
            var auditLogs = await _context.AuditTrails
                .OrderByDescending(a => a.DateTime) // ✅ CORRECT PROPERTY
                .ToListAsync();

            return View(auditLogs);
        }


        /* ======================================================= */

        /* Auto Mode pages */
        public IActionResult AutoMode()
        {
            return View();
        }

        /* Operation Mode pages */
        public IActionResult OperationMode()
        {
            return View();
        }

        /* Batch pages */
        public IActionResult Batch()
        {
            return View();
        }

        public IActionResult Calibration()
        {
            return View();
        }

        public IActionResult ViewBatch()
        {
            return View();
        }

        /* Manual Mode */
        public IActionResult ManualMode()
        {
            return View();
        }

        /* Signal Page */
        public IActionResult Signal()
        {
            return View();
        }
    }
}

using CompressionForce.Data;
using CompressionForce.Services;
using CompressionForce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


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

        /* ===================== AUTO TARE ===================== */
        public IActionResult AutoTare()
        {
            CompressionForce.Services.DTOs.AutoTareVm model =
                _autoTareService.GetStatus();

            return View(model);
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
            ViewBag.Users = await _context.UserManagements
                .Where(u => u.IsActive)
                .Select(u => u.ERname)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            var auditLogs = await _context.AuditTrails
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();

            return View(auditLogs);
        }

        /* Auto Mode pages */
        public IActionResult AutoMode()
        {
            return View();
        }

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

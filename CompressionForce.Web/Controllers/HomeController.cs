using CompressionForce.Data;
using CompressionForce.Services;
using CompressionForce.Models;
using CompressionForce.Web.Models;
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
        public async Task<IActionResult> AutoMode()
        {
            var vm = new AutoModeVM
            {
                ProductName = "No Active Batch",
                BatchNumber = "-",
                BatchQty = 0
            };

            try
            {
                // Read most recent CurrentBatch
                var current = await _context.CurrentBatches
                    .AsNoTracking()
                    .OrderByDescending(cb => cb.DateTime)
                    .FirstOrDefaultAsync();

                if (current != null && !string.IsNullOrWhiteSpace(current.BatchNumber))
                {
                    var batchNumber = current.BatchNumber.Trim();
                    vm.BatchNumber = batchNumber;

                    // Try both BatchCode and BatchNumber to handle schema mismatches
                    var batch = await _context.Batches
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b =>
                            (b.BatchCode != null && b.BatchCode == batchNumber) ||
                            (EF.Property<string>(b, "BatchNumber") != null && EF.Property<string>(b, "BatchNumber") == batchNumber)
                        );

                    if (batch != null)
                    {
                        vm.BatchQty = batch.BatchQty ?? 0;

                        // Resolve recipe/product name if available
                        var recipeCode = batch.RecipeCode;
                        if (!string.IsNullOrWhiteSpace(recipeCode))
                        {
                            var recipe = await _context.Recipes
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.RecipeCode == recipeCode);

                            if (recipe != null)
                                vm.ProductName = recipe.RecipeName;
                            else
                                vm.ProductName = "Active (recipe missing)";
                        }
                        else
                        {
                            vm.ProductName = "Active (no recipe code)";
                        }
                    }
                    else
                    {
                        // No matching batch row found — still show batch number
                        vm.ProductName = "Active (no batch record)";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Active Batch for AutoMode");
                vm.ProductName = "Error Loading Batch";
                vm.BatchNumber = "-";
                vm.BatchQty = 0;
            }

            return View(vm);
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

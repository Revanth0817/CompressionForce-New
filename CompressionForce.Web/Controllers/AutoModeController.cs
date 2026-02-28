using CompressionForce.Data;
using CompressionForce.Web.Models;
using CompressionForce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CompressionForce.Web.Controllers
{
    public class AutoModeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutoModeController> _logger;
        private readonly PlcMemoryCache _plcCache;

        public AutoModeController(
            ApplicationDbContext context,
            ILogger<AutoModeController> logger,
            PlcMemoryCache plcCache)
        {
            _context = context;
            _logger = logger;
            _plcCache = plcCache;
        }

        // GET: /AutoMode
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
                // load most recent current-batch row
                var current = await _context.CurrentBatches
                    .AsNoTracking()
                    .OrderByDescending(cb => cb.DateTime)
                    .FirstOrDefaultAsync();

                if (current != null && !string.IsNullOrWhiteSpace(current.BatchNumber))
                {
                    var batchNumber = current.BatchNumber.Trim();
                    vm.BatchNumber = batchNumber;

                    // try to find matching Batch by either BatchCode or BatchNumber
                    var batch = await _context.Batches
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.BatchCode == batchNumber || b.BatchNumber == batchNumber);

                    if (batch != null)
                    {
                        vm.BatchQty = batch.BatchQty ?? 0;

                        if (!string.IsNullOrWhiteSpace(batch.RecipeCode))
                        {
                            var recipe = await _context.Recipes
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.RecipeCode == batch.RecipeCode);

                            vm.ProductName = recipe?.RecipeName ?? "Active (recipe missing)";
                        }
                        else
                        {
                            vm.ProductName = "Active (no recipe code)";
                        }
                    }
                    else
                    {
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

            // Try to populate PLC cached values (non-blocking; cache returns defaults if absent)
            try
            {
                vm.GoodS1 = _plcCache.Get<int>("GOOD_S1");
                vm.RejectedS1 = _plcCache.Get<int>("REJECTED_S1");
                vm.TotalS1 = _plcCache.Get<int>("TOTAL_S1");

                vm.GoodS2 = _plcCache.Get<int>("GOOD_S2");
                vm.RejectedS2 = _plcCache.Get<int>("REJECTED_S2");
                vm.TotalS2 = _plcCache.Get<int>("TOTAL_S2");

                vm.AvgMain1 = _plcCache.Get<int>("AVG_MAIN_S1");
                vm.AvgPre1 = _plcCache.Get<int>("AVG_PRE_S1");
                vm.AvgEject1 = _plcCache.Get<int>("AVG_EJECT_S1");

                vm.AvgMain2 = _plcCache.Get<int>("AVG_MAIN_S2");
                vm.AvgPre2 = _plcCache.Get<int>("AVG_PRE_S2");
                vm.AvgEject2 = _plcCache.Get<int>("AVG_EJECT_S2");

                vm.DwellTime = _plcCache.Get<int>("DWELL_TIME");
                vm.TotalQty = _plcCache.Get<int>("TOTAL_QTY");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "PLC cache read failed (AutoMode initial load)");
            }

            return View(vm);
        }
    }
}
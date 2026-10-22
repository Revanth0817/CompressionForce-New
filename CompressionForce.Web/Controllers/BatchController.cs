using System.Text.RegularExpressions;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.Audit;
using CompressionForce.Services.Recipes;
using CompressionForce.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Web.Controllers
{
    /// <summary>
    /// JSON API behind the Batch screen (Home/Batch): list, add and deactivate batches.
    /// Every add / deactivate is also written to BatchHistories and the audit trail.
    /// </summary>
    [ApiController]
    [Route("Batch")]
    [RequirePrivilege("Batch")]
    public class BatchController : ControllerBase
    {
        public const string StatusActive = "New - Active";
        public const string StatusDeactivated = "Deactivated";

        private const int MaxBatchQty = 1_000_000;
        private static readonly Regex BatchCodeRule = new(@"^[A-Za-z0-9_\-]{1,20}$", RegexOptions.Compiled);

        private readonly ApplicationDbContext _context;
        private readonly IRecipeService _recipes;
        private readonly AuditLogger _audit;

        public BatchController(ApplicationDbContext context, IRecipeService recipes, AuditLogger audit)
        {
            _context = context;
            _recipes = recipes;
            _audit = audit;
        }

        private string CurrentUser => HttpContext.Session.GetString("UserName") ?? "Unknown";

        /// <summary>Recipe codes a batch can be created for.</summary>
        [HttpGet("Recipes")]
        public async Task<IActionResult> Recipes() => Ok(await _recipes.GetRecipeCodesAsync());

        /// <summary>All batches (newest first), optionally for one recipe.</summary>
        [HttpGet("List")]
        public async Task<IActionResult> List([FromQuery] string? recipe)
        {
            var query = _context.Batches.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(recipe))
                query = query.Where(b => b.RecipeCode == recipe);

            var batches = await query
                .OrderByDescending(b => b.DateTime)
                .ThenByDescending(b => b.Id)
                .Select(b => new
                {
                    id = b.Id,
                    dateTime = b.DateTime,
                    recipeCode = b.RecipeCode,
                    batchCode = b.BatchCode,
                    batchQty = b.BatchQty,
                    goodQty = b.GoodQty,
                    rejectionQty = b.RejectionQty,
                    status = b.BatchStatus,
                    userName = b.UserName,
                    isActive = b.BatchStatus != StatusDeactivated
                })
                .ToListAsync();

            return Ok(batches);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AddBatchDto dto)
        {
            if (dto == null)
                return BadRequest("Batch details are required");

            var recipeCode = dto.RecipeCode?.Trim();
            var batchCode = dto.BatchCode?.Trim();

            if (string.IsNullOrEmpty(recipeCode))
                return BadRequest("Select a recipe");

            if (string.IsNullOrEmpty(batchCode))
                return BadRequest("Enter a batch code");

            if (!BatchCodeRule.IsMatch(batchCode))
                return BadRequest("Batch code can have up to 20 letters, numbers, _ or -");

            if (dto.BatchQty is null or < 1 or > MaxBatchQty)
                return BadRequest($"Batch quantity must be between 1 and {MaxBatchQty:N0}");

            if (!await _recipes.ExistsByCodeAsync(recipeCode))
                return BadRequest($"Recipe '{recipeCode}' does not exist");

            var duplicate = await _context.Batches.AnyAsync(b =>
                b.RecipeCode == recipeCode &&
                b.BatchCode != null && b.BatchCode.ToLower() == batchCode.ToLower());
            if (duplicate)
                return Conflict($"Batch code '{batchCode}' already exists for recipe '{recipeCode}'");

            var qty = dto.BatchQty.Value;
            var now = DateTime.UtcNow;

            var batch = new Batch
            {
                DateTime = now,
                RecipeCode = recipeCode,
                BatchCode = batchCode,
                UserName = CurrentUser,
                BatchStatus = StatusActive,
                BatchQty = qty.ToString(),
                TabletQty = qty.ToString(),
                // counters start at zero, the PLC side updates them while the batch runs
                BatchSize = "0",
                ProdusedQty = "0",
                LeftQty = "0",
                GoodQty = "0",
                RejectionQty = "0",
                S2GoodQty = "0",
                S2RejectionQty = "0"
            };

            _context.Batches.Add(batch);
            _context.BatchHistories.Add(NewHistory(batch, qty, "ADD", now));
            await _context.SaveChangesAsync();

            _audit.Log("Batch", $"Batch '{batchCode}' added for recipe '{recipeCode}' (quantity {qty})");

            return Ok(new { id = batch.Id, batchCode, recipeCode });
        }

        [HttpPost("Deactivate")]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateBatchDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return BadRequest("Select a batch");

            var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == dto.Id);
            if (batch == null)
                return NotFound("Batch not found");

            if (batch.BatchStatus == StatusDeactivated)
                return Conflict($"Batch '{batch.BatchCode}' is already deactivated");

            long.TryParse(batch.BatchQty, out var qty);

            batch.BatchStatus = StatusDeactivated;
            _context.BatchHistories.Add(NewHistory(batch, qty, "DEACTIVATE", DateTime.UtcNow));
            await _context.SaveChangesAsync();

            _audit.Log("Batch", $"Batch '{batch.BatchCode}' of recipe '{batch.RecipeCode}' deactivated");

            return Ok();
        }

        private BatchHistory NewHistory(Batch batch, long qty, string eventType, DateTime at) => new()
        {
            DateTime = at,
            RecipeCode = batch.RecipeCode,
            BatchCode = batch.BatchCode,
            UserName = CurrentUser,
            BatchSize = qty,
            EReventType = eventType
        };
    }

    public class AddBatchDto
    {
        public string? RecipeCode { get; set; }
        public string? BatchCode { get; set; }
        public long? BatchQty { get; set; }
    }

    public class DeactivateBatchDto
    {
        public int Id { get; set; }
    }
}

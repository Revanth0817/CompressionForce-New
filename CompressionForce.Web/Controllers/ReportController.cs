using System.Reflection;
using CompressionForce.Data;
using CompressionForce.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Offline force report: reads the per-batch force results (one table per station and compression stage).
/// All six result tables have the same shape (Revolution, P1..P65, AverageLoad ...), so they share one loader.
/// </summary>
[RequirePrivilege("Report")]
public class ReportController : Controller
{
    private const int MaxRows = 5000;      // newest revolutions kept when a batch has more
    private const int MaxPunches = 65;     // P1..P65 columns

    private readonly ApplicationDbContext _db;

    public ReportController(ApplicationDbContext db) => _db = db;

    public IActionResult Report() => View();

    /// <summary>Batches that can be reported: every batch of the Batch screen plus any batch number found in the force tables.</summary>
    [HttpGet]
    public async Task<IActionResult> Batches()
    {
        var known = await _db.Batches.AsNoTracking()
            .OrderByDescending(b => b.DateTime).ThenByDescending(b => b.Id)
            .Select(b => new { code = b.BatchCode, recipe = b.RecipeCode, status = b.BatchStatus })
            .ToListAsync();

        var withData = new HashSet<string>(StringComparer.Ordinal);
        void Add(IEnumerable<string?> codes)
        {
            foreach (var c in codes)
                if (!string.IsNullOrWhiteSpace(c)) withData.Add(c!);
        }

        Add(await _db.ResultMainLoadS1Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());
        Add(await _db.ResultMainLoadS2Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());
        Add(await _db.ResultPreLoadS1Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());
        Add(await _db.ResultPreLoadS2Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());
        Add(await _db.ResultEjectLoadS1Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());
        Add(await _db.ResultEjectLoadS2Bs.Select(r => r.BatchNumber).Distinct().ToListAsync());

        var result = known
            .Where(b => !string.IsNullOrWhiteSpace(b.code))
            .Select(b => new { b.code, b.recipe, b.status, hasData = withData.Contains(b.code!) })
            .ToList();

        // batch numbers that only exist in the force tables
        var listed = new HashSet<string>(result.Select(r => r.code!), StringComparer.Ordinal);
        foreach (var code in withData.Where(c => !listed.Contains(c)).OrderBy(c => c))
            result.Add(new { code = (string?)code, recipe = (string?)null, status = (string?)null, hasData = true });

        return Json(result);
    }

    /// <summary>All recorded revolutions of one batch for one station/stage, oldest first.</summary>
    [HttpGet]
    public async Task<IActionResult> Data(string stage, string batch)
    {
        if (string.IsNullOrWhiteSpace(batch))
            return BadRequest("Select a batch");

        (List<ForceRow> rows, int total) loaded;
        switch ((stage ?? "").ToLowerInvariant())
        {
            case "s1-main": loaded = await Load(_db.ResultMainLoadS1Bs, batch); break;
            case "s2-main": loaded = await Load(_db.ResultMainLoadS2Bs, batch); break;
            case "s1-pre": loaded = await Load(_db.ResultPreLoadS1Bs, batch); break;
            case "s2-pre": loaded = await Load(_db.ResultPreLoadS2Bs, batch); break;
            case "s1-eject": loaded = await Load(_db.ResultEjectLoadS1Bs, batch); break;
            case "s2-eject": loaded = await Load(_db.ResultEjectLoadS2Bs, batch); break;
            default: return BadRequest("Unknown stage");
        }

        // how many punches actually carry data (the machine may have fewer than 65)
        var punchCount = 0;
        foreach (var row in loaded.rows)
            for (var i = MaxPunches - 1; i >= punchCount; i--)
                if (row.Values[i].HasValue) { punchCount = i + 1; break; }

        // only send the punch columns that exist (keeps big batches small)
        foreach (var row in loaded.rows)
            row.Values = row.Values.Take(punchCount).ToArray();

        return Json(new
        {
            batch,
            stage,
            total = loaded.total,
            truncated = loaded.total > loaded.rows.Count,
            punchCount,
            rows = loaded.rows
        });
    }

    private static async Task<(List<ForceRow>, int)> Load<T>(IQueryable<T> source, string batch) where T : class
    {
        var query = source.AsNoTracking().Where(e => EF.Property<string>(e, "BatchNumber") == batch);

        var total = await query.CountAsync();

        // newest revolutions win when there are more than the limit; the client wants them oldest first
        var entities = await query
            .OrderByDescending(e => EF.Property<long?>(e, "Revolution"))
            .ThenByDescending(e => EF.Property<int>(e, "Id"))
            .Take(MaxRows)
            .ToListAsync();
        entities.Reverse();

        var map = PropertyMap<T>.Instance;
        return (entities.Select(map.ToRow).ToList(), total);
    }

    /// <summary>JSON cannot carry NaN / Infinity</summary>
    private static double? Clean(double? v) => v.HasValue && double.IsFinite(v.Value) ? v : null;

    private sealed class PropertyMap<T> where T : class
    {
        public static readonly PropertyMap<T> Instance = new();

        private readonly PropertyInfo _id = typeof(T).GetProperty("Id")!;
        private readonly PropertyInfo _dateTime = typeof(T).GetProperty("DateTime")!;
        private readonly PropertyInfo _revolution = typeof(T).GetProperty("Revolution")!;
        private readonly PropertyInfo _average = typeof(T).GetProperty("AverageLoad")!;
        private readonly PropertyInfo _turret = typeof(T).GetProperty("TurretSpeed")!;
        private readonly PropertyInfo _srel = typeof(T).GetProperty("SrelValue")!;
        private readonly PropertyInfo[] _punches =
            Enumerable.Range(1, MaxPunches).Select(i => typeof(T).GetProperty("P" + i)!).ToArray();

        public ForceRow ToRow(T e)
        {
            var values = new double?[MaxPunches];
            for (var i = 0; i < MaxPunches; i++)
                values[i] = Clean((double?)_punches[i].GetValue(e));

            return new ForceRow
            {
                Id = (int)_id.GetValue(e)!,
                DateTime = (DateTime?)_dateTime.GetValue(e),
                Revolution = (long?)_revolution.GetValue(e),
                Average = Clean((double?)_average.GetValue(e)),
                TurretSpeed = Clean((double?)_turret.GetValue(e)),
                Srel = Clean((double?)_srel.GetValue(e)),
                Values = values
            };
        }
    }

    public sealed class ForceRow
    {
        public int Id { get; set; }
        public DateTime? DateTime { get; set; }
        public long? Revolution { get; set; }
        public double? Average { get; set; }
        public double? TurretSpeed { get; set; }
        public double? Srel { get; set; }
        public double?[] Values { get; set; } = Array.Empty<double?>();
    }
}

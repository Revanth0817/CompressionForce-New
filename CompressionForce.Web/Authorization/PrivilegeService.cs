using CompressionForce.Data;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Web.Authorization
{
    /// <summary>
    /// Resolves the privileges of the logged-in (session) user from their group.
    /// The "Admin" group implicitly holds every privilege so it can never lock itself out.
    /// </summary>
    public class PrivilegeService
    {
        public const string AdminGroup = "Admin";
        public const string AccessManagement = "AccessManagement";

        // Keys used by the sidebar menu / Access Management screen
        public static readonly string[] AllKeys =
        {
            "AccessManagement", "Alarm", "AuditTrail", "AutoMode", "AutoTareMode", "Batch",
            "Calibration", "ChangePassword", "Diagnostics", "HelpMenu", "Recipe", "Report"
        };

        private readonly ApplicationDbContext _context;

        public PrivilegeService(ApplicationDbContext context) => _context = context;

        public static bool IsAdminGroup(string? group) =>
            string.Equals(group, AdminGroup, StringComparison.OrdinalIgnoreCase);

        /// <summary>Active user's group, or null if not logged in / unknown / deactivated.</summary>
        public async Task<string?> GetGroupAsync(string? userName)
        {
            if (string.IsNullOrEmpty(userName)) return null;

            return await _context.UserManagements
                .AsNoTracking()
                .Where(u => u.ERname == userName && u.IsActive)
                .Select(u => u.ERlevel)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<string>> GetAllowedAsync(string? userName)
        {
            var group = await GetGroupAsync(userName);
            if (group == null) return Array.Empty<string>();
            if (IsAdminGroup(group)) return AllKeys;

            return await _context.GroupPrivileges
                .AsNoTracking()
                .Where(p => p.GroupName == group && p.IsAllowed)
                .Select(p => p.PrivilegeKey)
                .ToListAsync();
        }

        public async Task<bool> HasAsync(string? userName, string privilege) =>
            (await GetAllowedAsync(userName)).Contains(privilege);
    }
}

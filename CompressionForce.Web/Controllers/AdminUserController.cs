using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.Audit;
using BCrypt.Net;
using System.Text.RegularExpressions;
using CompressionForce.Web.Authorization;


namespace CompressionForce.Controllers
{
    [ApiController]
    [Route("AdminUser")]
    [RequirePrivilege(PrivilegeService.AccessManagement)]
    public class AdminUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogger _auditLogger;
        private readonly PrivilegeService _privileges;

        public AdminUserController(
            ApplicationDbContext context,
            AuditLogger auditLogger,
            PrivilegeService privileges)
        {
            _context = context;
            _auditLogger = auditLogger;
            _privileges = privileges;
        }

        private string? CurrentUser => HttpContext.Session.GetString("UserName");

        /* ================= CURRENT USER ================= */

        // Any logged-in user may read their own privileges (drives the sidebar menu)
        [HttpGet("MyPrivileges")]
        [SkipPrivilegeCheck]
        public async Task<IActionResult> MyPrivileges()
        {
            if (string.IsNullOrEmpty(CurrentUser)) return Unauthorized();
            return Ok(await _privileges.GetAllowedAsync(CurrentUser));
        }

        /* ================= USERS ================= */

        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            var now = DateTime.UtcNow;

            var users = await _context.UserManagements
                .OrderByDescending(u => u.CreatedDate)
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.ERname,
                    email = u.ERemail,
                    role = u.ERlevel,
                    isActive = u.IsActive &&
                               (u.ExpiryDate == null || u.ExpiryDate >= now),
                    isPending = u.IsPendingApproval,
                    createdDate = u.CreatedDate,
                    expiryDate = u.ExpiryDate,
                    lastLoginDate = u.LastLoginDate
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.Role))
                return BadRequest("All fields are required");

            var email = model.Email.Trim().ToLower();
            var username = model.UserName.Trim();

            if (!CompressionForce.Web.Security.PasswordPolicy.IsValid(model.Password))
                return BadRequest(CompressionForce.Web.Security.PasswordPolicy.Message);

            if (!Regex.IsMatch(username, @"^[A-Za-z0-9._-]{3,30}$"))
                return BadRequest("Username must be 3-30 characters: letters, numbers, dot, underscore or dash");

            if (!await _context.UserGroups.AnyAsync(g => g.Name == model.Role))
                return BadRequest("Unknown group");

            if (await _context.UserManagements.AnyAsync(u => u.ERemail == email))
                return BadRequest("Email already exists");

            if (await _context.UserManagements.AnyAsync(u => u.ERname == username))
                return BadRequest("Username already exists");

            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();
            var expiryDays = settings?.PasswordExpiryDays ?? 90;

            var user = new UserManagement
            {
                ERname = username,
                ERemail = email,
                ERpassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ERlevel = model.Role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(expiryDays),
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            _context.UserManagements.Add(user);
            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Admin created user '{username}' with role '{model.Role}'"
            );

            return Ok();
        }

        [HttpPost("ChangeAccess")]
        public async Task<IActionResult> ChangeAccess([FromBody] ChangeAccessDto dto)
        {
            var user = await _context.UserManagements.FindAsync(dto.UserId);
            if (user == null) return NotFound();

            if (user.ERname == CurrentUser)
                return BadRequest("You cannot change your own group");

            if (!await _context.UserGroups.AnyAsync(g => g.Name == dto.NewRole))
                return BadRequest("Unknown group");

            user.ERlevel = dto.NewRole;
            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Role changed for user '{user.ERname}' to '{dto.NewRole}'"
            );

            return Ok();
        }

        [HttpPost("SetActive")]
        public async Task<IActionResult> SetActive([FromBody] SetActiveDto dto)
        {
            var currentUser = CurrentUser;
            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id) &&
                            (dto.IsActive || u.ERname != currentUser)) // never let an admin lock themselves out
                .ToListAsync();

            var passwordDays = (await _context.SecuritySettings.AsNoTracking().FirstOrDefaultAsync())
                                   ?.PasswordExpiryDays ?? 90;
            var approved = new List<string>();

            foreach (var u in users)
            {
                if (dto.IsActive && u.IsPendingApproval)
                {
                    // approval: the password period starts now, not at sign-up
                    u.ExpiryDate = DateTime.UtcNow.AddDays(passwordDays);
                    approved.Add(u.ERname);
                }

                u.IsActive = dto.IsActive;
                u.IsPendingApproval = false;   // approved, or rejected: either way the request is dealt with
            }

            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Users {(dto.IsActive ? "activated" : "deactivated")} (IDs: {string.Join(",", dto.UserIds)})" +
                (approved.Count > 0 ? $"; approved sign-up of: {string.Join(", ", approved)}" : "")
            );

            return Ok();
        }

        /* ================= GROUPS ================= */

        [HttpGet("Groups")]
        public async Task<IActionResult> Groups()
        {
            return Ok(await _context.UserGroups
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync());
        }

        [HttpPost("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] string name)
        {
            name = name?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
                return BadRequest("Group name required");

            if (await _context.UserGroups.AnyAsync(g => g.Name == name))
                return BadRequest("Group already exists");

            _context.UserGroups.Add(new UserGroup { Name = name });
            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Group '{name}' created"
            );

            return Ok();
        }

        [HttpDelete("DeleteGroup/{name}")]
        public async Task<IActionResult> DeleteGroup(string name)
        {
            if (PrivilegeService.IsAdminGroup(name))
                return BadRequest("The Admin group cannot be deleted");

            var group = await _context.UserGroups.FirstOrDefaultAsync(g => g.Name == name);
            if (group == null) return BadRequest("Group not found");

            var inUse = await _context.UserManagements.AnyAsync(u => u.ERlevel == name);
            if (inUse) return BadRequest("Group assigned to users");

            _context.UserGroups.Remove(group);
            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Group '{name}' deleted"
            );

            return Ok();
        }

        /* ================= SECURITY SETTINGS ================= */

        // Defaults used until an administrator saves settings for the first time
        private static SecuritySettings DefaultSettings() => new()
        {
            ApplicationTimeoutMinutes = 10,
            PasswordExpiryDays = 90,
            MaxWrongAttempts = 3,
            LockoutMinutes = 30
        };

        [HttpGet("GetSecuritySettings")]
        public async Task<IActionResult> GetSecuritySettings()
        {
            var s = await _context.SecuritySettings.AsNoTracking().FirstOrDefaultAsync()
                    ?? DefaultSettings();

            return Ok(new
            {
                applicationTimeoutMinutes = s.ApplicationTimeoutMinutes,
                passwordExpiryDays = s.PasswordExpiryDays,
                maxWrongAttempts = s.MaxWrongAttempts,
                lockoutMinutes = s.LockoutMinutes
            });
        }

        [HttpPost("SaveSecuritySettings")]
        public async Task<IActionResult> SaveSecuritySettings([FromBody] SecuritySettingsDto dto)
        {
            if (dto == null)
                return BadRequest("Settings are required");

            if (dto.ApplicationTimeoutMinutes is < 1 or > 1440)
                return BadRequest("Application timeout must be between 1 and 1440 minutes");

            if (dto.PasswordExpiryDays is < 1 or > 365)
                return BadRequest("Password expiry must be between 1 and 365 days");

            if (dto.MaxWrongAttempts is < 1 or > 10)
                return BadRequest("Max wrong attempts must be between 1 and 10");

            if (dto.LockoutMinutes is < 1 or > 1440)
                return BadRequest("Lockout duration must be between 1 and 1440 minutes");

            var s = await _context.SecuritySettings.FirstOrDefaultAsync();
            if (s == null)
            {
                s = DefaultSettings();
                _context.SecuritySettings.Add(s);
            }

            var before = $"timeout={s.ApplicationTimeoutMinutes}m, expiry={s.PasswordExpiryDays}d, " +
                         $"attempts={s.MaxWrongAttempts}, lockout={s.LockoutMinutes}m";

            s.ApplicationTimeoutMinutes = dto.ApplicationTimeoutMinutes;
            s.PasswordExpiryDays = dto.PasswordExpiryDays;
            s.MaxWrongAttempts = dto.MaxWrongAttempts;
            if (dto.LockoutMinutes.HasValue) s.LockoutMinutes = dto.LockoutMinutes.Value;

            await _context.SaveChangesAsync();

            _auditLogger.Log(
                "Access Management",
                $"Security settings changed: [{before}] -> " +
                $"[timeout={s.ApplicationTimeoutMinutes}m, expiry={s.PasswordExpiryDays}d, " +
                $"attempts={s.MaxWrongAttempts}, lockout={s.LockoutMinutes}m]");

            return Ok();
        }

        // "Reset Password Expiry": restart the expiry clock of the selected users
        [HttpPost("ApplyPasswordExpiry")]
        public async Task<IActionResult> ApplyPasswordExpiry([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return BadRequest("Select at least one user");

            var days = (await _context.SecuritySettings.AsNoTracking().FirstOrDefaultAsync())
                           ?.PasswordExpiryDays ?? DefaultSettings().PasswordExpiryDays;

            var users = await _context.UserManagements
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (users.Count == 0)
                return NotFound("No matching users");

            var expiry = DateTime.UtcNow.AddDays(days);
            foreach (var u in users)
                u.ExpiryDate = expiry;

            await _context.SaveChangesAsync();

            _auditLogger.Log(
                "Access Management",
                $"Password expiry reset ({days} days) for users: {string.Join(", ", users.Select(u => u.ERname))}");

            return Ok();
        }

        // Admin sets a new password for the selected users
        [HttpPost("BulkChangePassword")]
        public async Task<IActionResult> BulkChangePassword([FromBody] BulkChangePasswordDto dto)
        {
            if (dto == null || dto.UserIds == null || dto.UserIds.Count == 0)
                return BadRequest("Select at least one user");

            // Same policy as Account/ChangePassword
            if (!CompressionForce.Web.Security.PasswordPolicy.IsValid(dto.NewPassword))
                return BadRequest(CompressionForce.Web.Security.PasswordPolicy.Message);

            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            if (users.Count == 0)
                return NotFound("No matching users");

            var days = (await _context.SecuritySettings.AsNoTracking().FirstOrDefaultAsync())
                           ?.PasswordExpiryDays ?? DefaultSettings().PasswordExpiryDays;

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var expiry = DateTime.UtcNow.AddDays(days);

            foreach (var u in users)
            {
                u.ERpassword = hash;
                u.ExpiryDate = expiry;
                u.FailedLoginAttempts = 0;
                u.LockedUntil = null;
            }

            await _context.SaveChangesAsync();

            // never log the password itself
            _auditLogger.Log(
                "Access Management",
                $"Password changed by administrator for users: {string.Join(", ", users.Select(u => u.ERname))}");

            return Ok();
        }

        /* ================= GROUP PRIVILEGES ================= */

        [HttpGet("GroupPrivileges/{group}")]
        public async Task<IActionResult> GroupPrivileges(string group)
        {
            if (PrivilegeService.IsAdminGroup(group))
                return Ok(PrivilegeService.AllKeys.Select(k => new { key = k, allowed = true }));

            var rows = await _context.GroupPrivileges
                .Where(p => p.GroupName == group)
                .Select(p => new { key = p.PrivilegeKey, allowed = p.IsAllowed })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpPost("SaveGroupPrivileges")]
        public async Task<IActionResult> SaveGroupPrivileges(
            [FromBody] SaveGroupPrivilegesDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GroupName))
                return BadRequest("Group required");

            if (PrivilegeService.IsAdminGroup(dto.GroupName))
                return BadRequest("The Admin group always has every privilege");

            if (!await _context.UserGroups.AnyAsync(g => g.Name == dto.GroupName))
                return BadRequest("Group not found");

            var old = await _context.GroupPrivileges
                .Where(p => p.GroupName == dto.GroupName)
                .ToListAsync();

            _context.GroupPrivileges.RemoveRange(old);

            foreach (var key in dto.Privileges ?? new List<string>())
            {
                _context.GroupPrivileges.Add(new GroupPrivilege
                {
                    GroupName = dto.GroupName,
                    PrivilegeKey = key,
                    IsAllowed = true
                });
            }

            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Privileges updated for group '{dto.GroupName}'"
            );

            return Ok();
        }
    }

    /* ================= DTOs ================= */

    public class CreateUserDto
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class ChangeAccessDto
    {
        public int UserId { get; set; }
        public string NewRole { get; set; } = "";
    }

    public class SetActiveDto
    {
        public List<int> UserIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class SecuritySettingsDto
    {
        public int ApplicationTimeoutMinutes { get; set; }
        public int PasswordExpiryDays { get; set; }
        public int MaxWrongAttempts { get; set; }
        public int? LockoutMinutes { get; set; }
    }

    public class BulkChangePasswordDto
    {
        public List<int> UserIds { get; set; } = new();
        public string NewPassword { get; set; } = "";
    }

    public class SaveGroupPrivilegesDto
    {
        public string GroupName { get; set; } = "";
        public List<string> Privileges { get; set; } = new();
    }
}

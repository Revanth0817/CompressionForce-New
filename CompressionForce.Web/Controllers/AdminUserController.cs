using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.Audit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Compression_Force.Controllers
{
    [ApiController]
    [Route("AdminUser")]
    public class AdminUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminUserController(ApplicationDbContext context)
        {
            _context = context;
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

                    // 🔴 AUTO INACTIVE IF EXPIRED
                    isActive = u.IsActive &&
                               (u.ExpiryDate == null || u.ExpiryDate >= now),

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

            return Ok();
        }

        [HttpPost("ChangeAccess")]
        public async Task<IActionResult> ChangeAccess([FromBody] ChangeAccessDto dto)
        {
            var user = await _context.UserManagements.FindAsync(dto.UserId);
            if (user == null) return NotFound();

            user.ERlevel = dto.NewRole;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("SetActive")]
        public async Task<IActionResult> SetActive([FromBody] SetActiveDto dto)
        {
            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var u in users)
                u.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
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
            return Ok();
        }

        [HttpDelete("DeleteGroup/{name}")]
        public async Task<IActionResult> DeleteGroup(string name)
        {
            var group = await _context.UserGroups.FirstOrDefaultAsync(g => g.Name == name);
            if (group == null) return BadRequest("Group not found");

            var inUse = await _context.UserManagements.AnyAsync(u => u.ERlevel == name);
            if (inUse) return BadRequest("Group assigned to users");

            _context.UserGroups.Remove(group);
            await _context.SaveChangesAsync();
            return Ok();
        }

        /* ================= GROUP PRIVILEGES ================= */

        [HttpGet("GroupPrivileges/{groupName}")]
        public async Task<IActionResult> GetGroupPrivileges(string groupName)
        {
            var data = await _context.GroupPrivileges
                .Where(p => p.GroupName == groupName)
                .Select(p => new
                {
                    key = p.PrivilegeKey,
                    allowed = p.IsAllowed
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("SaveGroupPrivileges")]
        public async Task<IActionResult> SaveGroupPrivileges(
            [FromBody] SaveGroupPrivilegesDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GroupName))
                return BadRequest("Group required");

            var old = await _context.GroupPrivileges
                .Where(p => p.GroupName == dto.GroupName)
                .ToListAsync();

            _context.GroupPrivileges.RemoveRange(old);

            foreach (var key in dto.Privileges)
            {
                _context.GroupPrivileges.Add(new GroupPrivilege
                {
                    GroupName = dto.GroupName,
                    PrivilegeKey = key,
                    IsAllowed = true
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        /* ================= 🔐 SECURITY SETTINGS ================= */

        [HttpGet("GetSecuritySettings")]
        public async Task<IActionResult> GetSecuritySettings()
        {
            var s = await _context.SecuritySettings.FirstOrDefaultAsync();

            if (s == null)
            {
                s = new SecuritySettings
                {
                    ApplicationTimeoutMinutes = 10,
                    PasswordExpiryDays = 10,
                    MaxWrongAttempts = 10
                };

                _context.SecuritySettings.Add(s);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                applicationTimeoutMinutes = s.ApplicationTimeoutMinutes,
                passwordExpiryDays = s.PasswordExpiryDays,
                maxWrongAttempts = s.MaxWrongAttempts
            });
        }

        [HttpPost("SaveSecuritySettings")]
        public async Task<IActionResult> SaveSecuritySettings(
            [FromBody] SecuritySettingsDto dto)
        {
            if (dto.ApplicationTimeoutMinutes <= 0 ||
                dto.PasswordExpiryDays <= 0 ||
                dto.MaxWrongAttempts <= 0)
                return BadRequest("Invalid values");

            var s = await _context.SecuritySettings.FirstOrDefaultAsync()
                    ?? new SecuritySettings();

            s.ApplicationTimeoutMinutes = dto.ApplicationTimeoutMinutes;
            s.PasswordExpiryDays = dto.PasswordExpiryDays;
            s.MaxWrongAttempts = dto.MaxWrongAttempts;

            _context.SecuritySettings.Update(s);
            await _context.SaveChangesAsync();
            return Ok();
        }

        /* ================= 🔑 APPLY PASSWORD EXPIRY ================= */

        [HttpPost("ApplyPasswordExpiry")]
        public async Task<IActionResult> ApplyPasswordExpiry(
            [FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return BadRequest("No users selected");

            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();
            if (settings == null)
                return BadRequest("Security settings not found");

            var expiryDate = DateTime.UtcNow.AddDays(settings.PasswordExpiryDays);

            var users = await _context.UserManagements
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
                user.ExpiryDate = expiryDate;

            await _context.SaveChangesAsync();
            return Ok();
        }

        /* ================= 🔐 BULK CHANGE PASSWORD ================= */

        [HttpPost("BulkChangePassword")]
        public async Task<IActionResult> BulkChangePassword(
            [FromBody] BulkChangePasswordDto dto)
        {
            if (dto.UserIds == null || dto.UserIds.Count == 0)
                return BadRequest("No users selected");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Password required");

            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.ERpassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        /* ================= MY PRIVILEGES (CURRENT USER) ================= */

        [HttpGet("MyPrivileges")]
        public async Task<IActionResult> MyPrivileges()
        {
            var role = HttpContext.Session.GetString("UserLevel");

            if (string.IsNullOrEmpty(role))
                return Unauthorized("Not logged in");

            var allowed = await _context.GroupPrivileges
                .Where(p => p.GroupName == role && p.IsAllowed)
                .Select(p => p.PrivilegeKey)
                .ToListAsync();

            return Ok(allowed); // e.g. ["Recipe","Diagnostics","Batch"]
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

    public class SaveGroupPrivilegesDto
    {
        public string GroupName { get; set; } = "";
        public List<string> Privileges { get; set; } = new();
    }

    public class SecuritySettingsDto
    {
        public int ApplicationTimeoutMinutes { get; set; }
        public int PasswordExpiryDays { get; set; }
        public int MaxWrongAttempts { get; set; }
    }

    public class BulkChangePasswordDto
    {
        public List<int> UserIds { get; set; } = new();
        public string NewPassword { get; set; } = "";
    }
}
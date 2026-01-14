using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Services.Audit;
using BCrypt.Net;


namespace CompressionForce.Controllers
{
    [ApiController]
    [Route("AdminUser")]
    public class AdminUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogger _auditLogger;

        public AdminUserController(
            ApplicationDbContext context,
            AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
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
            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var u in users)
                u.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            // 🔥 AUDIT LOG
            _auditLogger.Log(
                "Access Management",
                $"Users {(dto.IsActive ? "activated" : "deactivated")} (IDs: {string.Join(",", dto.UserIds)})"
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

        /* ================= GROUP PRIVILEGES ================= */

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

    public class SaveGroupPrivilegesDto
    {
        public string GroupName { get; set; } = "";
        public List<string> Privileges { get; set; } = new();
    }
}

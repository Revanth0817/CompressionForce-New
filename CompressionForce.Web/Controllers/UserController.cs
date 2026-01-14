using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompressionForce.Data;

namespace CompressionForce.Controllers
{
    [ApiController]
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 LIST USERS
        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            var users = await _context.UserManagements
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.ERname,
                    email = u.ERemail,
                    role = u.ERlevel,
                    isActive = u.IsActive,
                    createdDate = u.CreatedDate,
                    expiryDate = u.ExpiryDate,
                    lastLoginDate = u.LastLoginDate
                })
                .ToListAsync();

            return Ok(users);
        }

        // 🔹 ACTIVATE / DEACTIVATE
        [HttpPost("SetActive")]
        public async Task<IActionResult> SetActive([FromBody] ActivateUserDto dto)
        {
            var users = await _context.UserManagements
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = dto.IsActive;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class ActivateUserDto
    {
        public List<int> UserIds { get; set; } = new();
        public bool IsActive { get; set; }
    }
}

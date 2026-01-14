using Microsoft.AspNetCore.Mvc;
using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using CompressionForce.Models;
using BCrypt.Net;

namespace CompressionForce.Controllers
{
    [ApiController]
    [Route("Register")]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Register/Create
        [HttpPost("Create")]
        public IActionResult Create([FromBody] RegisterDto model)
        {
            if (model == null)
                return BadRequest("Request body is empty");

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required");

            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new UserManagement
            {
                ERname = model.UserName,
                ERemail = model.Email, // ✅ FIXED
                ERpassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ERlevel = model.Role,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserManagements.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                userName = user.ERname,
                email = user.ERemail,
                role = user.ERlevel,
                createdDate = user.CreatedDate,
                isActive = true
            });
        }
    }
}

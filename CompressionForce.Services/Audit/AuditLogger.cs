using CompressionForce.Data;
using CompressionForce.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace CompressionForce.Services.Audit
{
    public class AuditLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Log(string activity, string description)
        {
            var userName =
                _httpContextAccessor.HttpContext?.Session.GetString("UserName")
                ?? "Unknown";

            var audit = new AuditTrail
            {
                UserName = userName,
                Activity = activity,
                EventDescription = description,
                DateTime = DateTime.UtcNow
            };

            _context.AuditTrails.Add(audit);
            _context.SaveChanges();
        }
    }
}

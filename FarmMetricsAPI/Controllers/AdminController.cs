using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmMetricsAPI.Models.Postgres;
using FarmMetricsAPI.Data;
using System.Linq;

namespace FarmMetricsAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(string filter = "")
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Settlement)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => 
                    u.Name.Contains(filter) || 
                    u.Email.Contains(filter) || 
                    u.Phone.Contains(filter));
            }

            var users = await query
                .Select(u => new 
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    Role = u.Role.Name,
                    Settlement = u.Settlement.Name,
                    IsBanned = u.Name.StartsWith("[BANNED]")
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("users/{userId}/ban")]
        public async Task<IActionResult> BanUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.Name.StartsWith("[BANNED]"))
            {
                user.Name = $"[BANNED] {user.Name}";
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("users/{userId}/unban")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Name.StartsWith("[BANNED]"))
            {
                user.Name = user.Name.Replace("[BANNED] ", "");
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
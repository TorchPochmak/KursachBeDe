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

        private readonly string BanGuid = GenerateSeededGuid(29052025).ToString();
        public AdminController(AppDbContext context)
        {
            _context = context;
        }
        public static Guid GenerateSeededGuid(int seed)
        {
            var r = new Random(seed);
            var guid = new byte[16];
            r.NextBytes(guid);
            return new Guid(guid);
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
                    IsBanned = u.Name.StartsWith($"[{BanGuid}]")
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

            if (!user.Name.StartsWith($"[{BanGuid}]"))
            {
                user.Name = $"[{BanGuid}] {user.Name}";
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

            if (user.Name.StartsWith($"[{BanGuid}]"))
            {
                user.Name = user.Name.Replace($"[{BanGuid}] ", "");
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
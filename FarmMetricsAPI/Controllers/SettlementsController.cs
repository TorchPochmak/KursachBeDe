using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Models.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettlementsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settlements = await _context.Settlements
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(settlements);
        }

        [HttpPut("user/{userId}/settlement")]
        public async Task<IActionResult> UpdateUserSettlement(int userId, [FromBody] UpdateSettlementRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var settlement = await _context.Settlements.FindAsync(request.SettlementId);
            if (settlement == null)
            {
                return NotFound("Settlement not found");
            }

            user.SettlementId = request.SettlementId;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("user/{userId}/settlement")]
        public async Task<IActionResult> RemoveUserSettlement(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.SettlementId = null;
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> AddSettlement([FromBody] AddSettlementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name required");

            var exists = await _context.Settlements.AnyAsync(s => s.Name == request.Name);
            if (exists)
                return BadRequest("Settlement already exists");

            var settlement = new Settlement { Name = request.Name };
            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            return Ok(new { settlement.Id, settlement.Name });
        }

        public class AddSettlementRequest
        {
            public string Name { get; set; } = string.Empty;
        }


        public class UpdateSettlementRequest
        {
            public int SettlementId { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MetricsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var metrics = await _dbContext.Metrics
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.MinValue,
                    m.MaxValue
                })
                .ToListAsync();

            return Ok(metrics);
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using MongoDB.Driver;
using FarmMetricsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/farm")]
public class FarmController : ControllerBase
{
    private readonly MongoDbContext _mongoContext;
    private readonly AppDbContext _context;

    public FarmController(MongoDbContext mongoContext, AppDbContext context)
    {
        _mongoContext = mongoContext;
        _context = context;
    }

    private async Task<List<string>> GetAvailableMetricNames(int settlementId)
    {
        return await _context.SettleMetricDevices
            .Where(d => d.SettlementId == settlementId)
            .Include(d => d.Metric)
            .Select(d => d.Metric.Name)
            .ToListAsync();
    }

    private bool ValidateMetrics(List<MongoMetric> metrics, List<string> availableMetricNames)
    {
        return metrics == null || metrics.All(m => availableMetricNames.Contains(m.Name));
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll([FromQuery] int userId)
    {
        var farms = await _mongoContext.Farms
            .Find(f => f.UserId == userId)
            .ToListAsync();
        return Ok(farms);
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int id)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        return Ok(farm);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MongoFarm farm)
    {
        // Get available metrics for the settlement
        var availableMetricNames = await GetAvailableMetricNames(farm.SettlementId);

        // Validate that all metrics in the farm are available in the settlement
        if (!ValidateMetrics(farm.Metrics, availableMetricNames))
        {
            return BadRequest($"Some metrics are not available in this settlement. Available metrics: {string.Join(", ", availableMetricNames)}");
        }

        // Initialize collections if they're null
        farm.Cultures ??= new List<MongoCulture>();
        farm.Comments ??= new List<MongoComment>();
        farm.Harvests ??= new List<MongoHarvest>();
        farm.Metrics ??= new List<MongoMetric>();

        await _mongoContext.Farms.InsertOneAsync(farm);
        return Ok(new { Id = farm.Id });
    }

    [HttpPost("change")]
    public async Task<IActionResult> Change([FromQuery] int id, [FromBody] MongoFarm farmChanges)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        // Get available metrics for the settlement
        var availableMetricNames = await GetAvailableMetricNames(farmChanges.SettlementId);

        // Validate that all metrics in the farm changes are available in the settlement
        if (!ValidateMetrics(farmChanges.Metrics, availableMetricNames))
        {
            return BadRequest($"Some metrics are not available in this settlement. Available metrics: {string.Join(", ", availableMetricNames)}");
        }

        var update = Builders<MongoFarm>.Update
            .Set(f => f.Name, farmChanges.Name)
            .Set(f => f.UserId, farmChanges.UserId)
            .Set(f => f.SettlementId, farmChanges.SettlementId)
            .Set(f => f.Cultures, farmChanges.Cultures ?? farm.Cultures)
            .Set(f => f.Metrics, farmChanges.Metrics ?? farm.Metrics)
            .Set(f => f.Harvests, farmChanges.Harvests ?? farm.Harvests);

        await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            update
        );

        return Ok();
    }

    [HttpPost("addprop")]
    public async Task<IActionResult> AddProperty([FromQuery] int id, [FromBody] Dictionary<string, object> properties)
    {
        var update = Builders<MongoFarm>.Update;
        var updates = new List<UpdateDefinition<MongoFarm>>();

        foreach (var prop in properties)
        {
            updates.Add(update.Set(prop.Key, prop.Value));
        }

        if (updates.Count == 0)
            return BadRequest("No properties to add");

        var combinedUpdate = update.Combine(updates);
        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            combinedUpdate
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found or no changes made");

        return Ok();
    }

    [HttpPost("deleteprop")]
    public async Task<IActionResult> DeleteProperty([FromQuery] int id, [FromBody] List<string> propertyNames)
    {
        var protectedProps = new[] { "id", "settlementid", "cultures", "userId", "name", "metrics", "harvests", "comments" };
        var invalidProps = propertyNames.Select(x => x.ToLower()).Intersect(protectedProps).ToList();

        if (invalidProps.Any())
            return BadRequest($"Cannot delete protected properties: {string.Join(", ", invalidProps)}");

        var update = Builders<MongoFarm>.Update;
        var updates = propertyNames.Select(prop => update.Unset(prop));
        var combinedUpdate = update.Combine(updates);

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            combinedUpdate
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found or no changes made");

        return Ok();
    }

    [HttpGet("available-metrics")]
    public async Task<IActionResult> GetAvailableMetrics([FromQuery] int settlementId)
    {
        var availableMetrics = await _context.SettleMetricDevices
            .Where(d => d.SettlementId == settlementId)
            .Include(d => d.Metric)
            .Select(d => new
            {
                d.Metric.Name,
                d.Metric.MinValue,
                d.Metric.MaxValue
            })
            .ToListAsync();

        return Ok(availableMetrics);
    }

    [HttpGet("bysettlement")]
    public async Task<IActionResult> GetBySettlement([FromQuery] int settlementId)
    {
        var farms = await _mongoContext.Farms
            .Find(f => f.SettlementId == settlementId)
            .ToListAsync();
        return Ok(farms);
    }
}
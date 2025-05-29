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
    public async Task<IActionResult> Get([FromQuery] string id)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        return Ok(farm);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromQuery] string name, [FromQuery] int settlementId, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name is required");

        var farm = new MongoFarm
        {
            Name = name,
            SettlementId = settlementId,
            UserId = userId,
            Cultures = new List<MongoCulture>(),
            Comments = new List<MongoComment>(),
            Harvests = new List<MongoHarvest>(),
            Metrics = new List<MongoMetric>()
        };

        await _mongoContext.Farms.InsertOneAsync(farm);
        return Ok(new { Id = farm.Id });
    }


    [HttpGet("available-metrics")]
    public async Task<IActionResult> GetAvailableMetrics([FromQuery] int settlementId)
    {
        var availableMetrics = await _context.SettleMetricDevices
            .Where(d => d.SettlementId == settlementId)
            .Include(d => d.Metric)
            .Select(d => new
            {
                d.Id,
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

    [HttpPost("metrics/add")]
    public async Task<IActionResult> AddMetric([FromQuery] string farmId, [FromQuery] int deviceId, [FromQuery] double value)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == farmId)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        // Получаем устройство и проверяем его доступность
        var device = await _context.SettleMetricDevices
            .Include(d => d.Metric)
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.SettlementId == farm.SettlementId);

        if (device == null)
            return BadRequest("Device not found or not available in this settlement");

        var metric = new MongoMetric 
        { 
            Name = device.Metric.Name,
            Value = value,
            DeviceId = deviceId
        };

        var update = Builders<MongoFarm>.Update.Push(f => f.Metrics, metric);
        await _mongoContext.Farms.UpdateOneAsync(f => f.Id == farmId, update);

        return Ok();
    }

    [HttpDelete("metrics/delete")]
    public async Task<IActionResult> DeleteMetric([FromQuery] string farmId, [FromQuery] int deviceId)
    {
        var update = Builders<MongoFarm>.Update.PullFilter(
            f => f.Metrics,
            m => m.DeviceId == deviceId
        );

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm or metric not found");

        return Ok();
    }

    [HttpPost("harvests/add")]
    public async Task<IActionResult> AddHarvest([FromQuery] string farmId, [FromQuery] string name, [FromQuery] string info)
    {
        var harvest = new MongoHarvest
        {
            Name = name,
            Info = info,
            RegisteredAt = DateTime.UtcNow
        };

        var update = Builders<MongoFarm>.Update.Push(f => f.Harvests, harvest);
        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found");

        return Ok();
    }

    [HttpDelete("harvests/delete")]
    public async Task<IActionResult> DeleteHarvest([FromQuery] string farmId, [FromQuery] string harvestId)
    {
        var update = Builders<MongoFarm>.Update.PullFilter(
            f => f.Harvests,
            h => h.Id == harvestId
        );

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm or harvest not found");

        return Ok();
    }

    [HttpPost("comments/add")]
    public async Task<IActionResult> AddComment([FromQuery] string farmId, [FromQuery] string info)
    {
        var comment = new MongoComment
        {
            Info = info,
            Date = DateTime.UtcNow
        };

        var update = Builders<MongoFarm>.Update.Push(f => f.Comments, comment);
        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found");

        return Ok();
    }

    [HttpDelete("comments/delete")]
    public async Task<IActionResult> DeleteComment([FromQuery] string farmId, [FromQuery] string commentId)
    {
        var update = Builders<MongoFarm>.Update.PullFilter(
            f => f.Comments,
            c => c.Id == commentId
        );

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm or comment not found");

        return Ok();
    }

    [HttpPost("cultures/add")]
    public async Task<IActionResult> AddCulture([FromQuery] string farmId, [FromQuery] string name, [FromQuery] int squareMeters)
    {
        var culture = new MongoCulture
        {
            Name = name,
            SquareMeters = squareMeters
        };

        var update = Builders<MongoFarm>.Update.Push(f => f.Cultures, culture);
        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found");

        return Ok();
    }

    [HttpDelete("cultures/delete")]
    public async Task<IActionResult> DeleteCulture([FromQuery] string farmId, [FromQuery] string cultureName)
    {
        var update = Builders<MongoFarm>.Update.PullFilter(
            f => f.Cultures,
            c => c.Name == cultureName
        );

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm or culture not found");

        return Ok();
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFarm([FromQuery] string farmId)
    {
        var result = await _mongoContext.Farms.DeleteOneAsync(f => f.Id == farmId);

        if (result.DeletedCount == 0)
            return NotFound("Farm not found");

        return Ok();
    }
}
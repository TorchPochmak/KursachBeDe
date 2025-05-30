using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using FarmMetricsAPI.Models.Postgres;
using StackExchange.Redis;
using System.Text.Json;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly MongoDbContext _mongoContext;
    private readonly IConnectionMultiplexer _redis;

    public DevicesController(
        AppDbContext dbContext, 
        MongoDbContext mongoContext,
        IConnectionMultiplexer redis)
    {
        _dbContext = dbContext;
        _mongoContext = mongoContext;
        _redis = redis;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll([FromQuery] int settlementId)
    {
        var devices = await _dbContext.SettleMetricDevices
    .Where(d => d.SettlementId == settlementId)
    .Include(d => d.Metric)
    .Include(d => d.Settlement)
    .ToListAsync();

        var result = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            MetricName = d.Metric?.Name ?? "",
            MinValue = d.Metric?.MinValue ?? 0,
            MaxValue = d.Metric?.MaxValue ?? 0,
            SettlementName = d.Settlement?.Name ?? "",
            RegisteredAt = d.RegisteredAt
        }).ToList();

        return Ok(result);

    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] int? settlementId = null,
        [FromQuery] int? metricId = null)
    {
        var query = _dbContext.SettleMetricDevices.AsQueryable();

        if (settlementId.HasValue)
        {
            query = query.Where(d => d.SettlementId == settlementId.Value);
        }

        if (metricId.HasValue)
        {
            query = query.Where(d => d.MetricId == metricId.Value);
        }

        if (!settlementId.HasValue && !metricId.HasValue)
        {
            return BadRequest("At least one search parameter (settlementId or metricId) must be provided");
        }

        var devices = await query
            .Include(d => d.Metric)
            .Include(d => d.Settlement)
            .ToListAsync();

        var result = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            MetricName = d.Metric?.Name ?? "",
            MinValue = d.Metric?.MinValue ?? 0,
            MaxValue = d.Metric?.MaxValue ?? 0,
            SettlementName = d.Settlement?.Name ?? "",
            RegisteredAt = d.RegisteredAt
        }).ToList();

        return Ok(result);
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetDeviceAverage([FromQuery] int deviceId)
    {
        var device = await _dbContext.SettleMetricDevices
            .Include(d => d.Metric)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
            return NotFound("Device not found");


        var db = _redis.GetDatabase();
        var today = DateTime.UtcNow.Date;
        var cacheKey = $"device_avg:{deviceId}:{today:yyyy-MM-dd}";

        var cachedValue = await db.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            return Ok(JsonSerializer.Deserialize<DeviceAverageDto>(cachedValue));
        }


        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var metricData = await _dbContext.MetricData
            .Where(md => md.SettleMetricDeviceId == deviceId && 
                        md.RegisteredAt >= sevenDaysAgo)
            .ToListAsync();

        if (!metricData.Any())
            return NotFound("No metric data found for the last 7 days");

        var average = metricData.Average(md => md.MetricValue);
        var result = new DeviceAverageDto
        {
            DeviceId = deviceId,
            MetricName = device.Metric.Name,
            AverageValue = average,
            CalculatedAt = DateTime.UtcNow,
            DataPointsCount = metricData.Count,
            PeriodStart = sevenDaysAgo,
            PeriodEnd = DateTime.UtcNow
        };


        await db.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            TimeSpan.FromHours(1)
        );

        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int deviceId)
    {
        var device = await _dbContext.SettleMetricDevices
            .Include(d => d.Metric)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
            return NotFound("Device not found");


        var isLastDeviceForMetric = !await _dbContext.SettleMetricDevices
            .AnyAsync(d => d.Id != deviceId && 
                          d.SettlementId == device.SettlementId && 
                          d.MetricId == device.MetricId);

        if (isLastDeviceForMetric)
        {

            var filter = Builders<MongoFarm>.Filter.Eq(f => f.SettlementId, device.SettlementId);
            var update = Builders<MongoFarm>.Update.PullFilter(
                f => f.Metrics,
                Builders<MongoMetric>.Filter.Eq(m => m.Name, device.Metric.Name)
            );

            await _mongoContext.Farms.UpdateManyAsync(filter, update);
        }


        _dbContext.SettleMetricDevices.Remove(device);
        await _dbContext.SaveChangesAsync();

        return Ok(new 
        { 
            message = isLastDeviceForMetric 
                ? $"Device deleted and metric '{device.Metric.Name}' removed from all farms in settlement" 
                : "Device deleted" 
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] SettleMetricDevice device)
    {
        await _dbContext.SettleMetricDevices.AddAsync(device);
        await _dbContext.SaveChangesAsync();
        return Ok(device);
    }

    public class DeviceDto
    {
        public int Id { get; set; }
        public string MetricName { get; set; } = "";
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string SettlementName { get; set; } = "";
        public DateTime RegisteredAt { get; set; }
    }

    public class DeviceAverageDto
    {
        public int DeviceId { get; set; }
        public string MetricName { get; set; } = "";
        public double AverageValue { get; set; }
        public DateTime CalculatedAt { get; set; }
        public int DataPointsCount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
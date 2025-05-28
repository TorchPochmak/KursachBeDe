using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using FarmMetricsAPI.Models.Postgres;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly MongoDbContext _mongoContext;

    public DevicesController(AppDbContext dbContext, MongoDbContext mongoContext)
    {
        _dbContext = dbContext;
        _mongoContext = mongoContext;
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



    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int deviceId)
    {
        var device = await _dbContext.SettleMetricDevices.FindAsync(deviceId);
        if (device == null)
            return NotFound("Device not found");

        // First, find all farms that have this device's metric
        var farms = await _mongoContext.Farms
            .Find(f => f.SettlementId == device.SettlementId)
            .ToListAsync();

        // Remove the metric from all farms
        foreach (var farm in farms)
        {
            var metricsToRemove = farm.Metrics
                .Where(m => m.Name == device.Metric?.Name)
                .ToList();

            foreach (var metric in metricsToRemove)
            {
                farm.Metrics.Remove(metric);
            }

            await _mongoContext.Farms.ReplaceOneAsync(
                f => f.Id == farm.Id,
                farm
            );
        }

        // Now delete the device from PostgreSQL (this will cascade delete related metric data)
        _dbContext.SettleMetricDevices.Remove(device);
        await _dbContext.SaveChangesAsync();
        return Ok();
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

}
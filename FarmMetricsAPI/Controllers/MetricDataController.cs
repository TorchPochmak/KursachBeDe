using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/metricdata")]
public class MetricDataController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MetricDataController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Получает все измерения метрик для указанного поселения
    /// </summary>
    /// <param name="settlementId">Идентификатор поселения</param>
    /// <returns>Список измерений метрик с информацией об устройствах</returns>
    [HttpGet("{settlementId}/getall")]
    public async Task<IActionResult> GetAll(int settlementId)
    {
        var metricData = await _dbContext.MetricData
            .Where(md => md.Device.SettlementId == settlementId)
            .OrderByDescending(md => md.RegisteredAt)
            .Select(md => new 
            {
                md.Id,
                md.RegisteredAt,
                md.MetricValue,
                md.SettleMetricDeviceId,
                MetricName = md.Device.Metric.Name
            })
            .ToListAsync();

        return Ok(metricData);
    }
} 
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Models.Postgres;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace FarmMetricsAPI.Jobs;

public class AddMetricDataJob : IJob
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public AddMetricDataJob(AppDbContext context)
    {
        _context = context;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var settleMetricDevices = await _context.SettleMetricDevices.Include(x => x.Metric).ToListAsync();
        var currentTime = DateTime.Now;
        // Round to the current hour
        var roundedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 
            currentTime.Hour, 0, 0);

        foreach (var device in settleMetricDevices)
        {
            if (!await _context.MetricData.AnyAsync(md =>
                md.SettleMetricDeviceId == device.Id &&
                md.RegisteredAt == roundedTime))
            {
                var metricData = new MetricData
                {
                    SettleMetricDeviceId = device.Id,
                    RegisteredAt = roundedTime,
                    MetricValue = GetMetricValue(device.Metric)
                };
                await _context.MetricData.AddAsync(metricData);
            }
        }
        await _context.SaveChangesAsync();
    }

    private double GetMetricValue(Metric metric)
    {
        return metric.MinValue + (_random.NextDouble() * (metric.MaxValue - metric.MinValue));
    }
} 
using FarmMetricsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace FarmMetricsAPI.Jobs;

public class MetricDataCleanerJob : IJob
{
    private readonly AppDbContext _context;
    private readonly ILogger<MetricDataCleanerJob> _logger;
    private const int RetentionDays = 30; // Keep data for 30 days

    public MetricDataCleanerJob(AppDbContext context, ILogger<MetricDataCleanerJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-RetentionDays).Date;
            
            // Delete old metric data
            var deletedCount = await _context.MetricData
                .Where(md => md.RegisteredAt < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Cleaned {Count} old metric data records older than {Date}", 
                deletedCount, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning old metric data");
            throw;
        }
    }
} 
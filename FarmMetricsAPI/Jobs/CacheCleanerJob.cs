using Quartz;
using StackExchange.Redis;

namespace FarmMetricsAPI.Jobs;

public class CacheCleanerJob : IJob
{
    private readonly IConnectionMultiplexer _redis;
    private const int MAX_CACHE_ENTRIES = 10_000_000;

    public CacheCleanerJob(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        
        // Get all keys matching our pattern
        var keys = server.Keys(pattern: "device_avg:*").ToList();
        
        if (keys.Count > MAX_CACHE_ENTRIES)
        {
            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var keysToDelete = keys.Where(k => !k.ToString().EndsWith(today)).ToList();
            
            if (keysToDelete.Any())
            {
                await db.KeyDeleteAsync(keysToDelete.ToArray());
                Console.WriteLine($"Cleaned {keysToDelete.Count} cache entries from Redis");
            }
        }
    }
} 
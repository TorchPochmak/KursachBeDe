namespace FarmMetricsAPI.Models.Mongo;
public class MongoHarvest
{
    public DateTime RegisteredAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
} 
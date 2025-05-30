using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmMetricsAPI.Models.Mongo;

public class MongoMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public double Value { get; set; }

    public int DeviceId { get; set; }
} 
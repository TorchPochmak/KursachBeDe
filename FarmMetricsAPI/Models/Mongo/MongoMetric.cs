using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmMetricsAPI.Models.Mongo;

public class MongoMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    
    public double Value { get; set; }
} 
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmMetricsAPI.Models.Mongo;


public class MongoHarvest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public DateTime RegisteredAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
} 
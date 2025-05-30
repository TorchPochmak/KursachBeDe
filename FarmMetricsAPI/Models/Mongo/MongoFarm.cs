using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmMetricsAPI.Models.Mongo;

public class MongoFarm
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public int UserId { get; set; }

    public int SettlementId { get; set; }

    public List<MongoCulture> Cultures { get; set; } = new();

    public List<MongoMetric> Metrics { get; set; } = new();

    public List<MongoHarvest> Harvests { get; set; } = new();

    public List<MongoComment> Comments { get; set; } = new();
}
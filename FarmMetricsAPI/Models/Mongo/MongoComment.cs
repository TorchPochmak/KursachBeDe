using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmMetricsAPI.Models.Mongo;

public class MongoComment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Info { get; set; } = "";
}
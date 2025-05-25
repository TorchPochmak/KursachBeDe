using Microsoft.Extensions.Options;
using MongoDB.Driver;
using FarmMetricsAPI.Models.Mongo;

namespace FarmMetricsAPI.Data.MongoDb;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<MongoFarm> Farms => _database.GetCollection<MongoFarm>(_settings.FarmsCollectionName);
} 
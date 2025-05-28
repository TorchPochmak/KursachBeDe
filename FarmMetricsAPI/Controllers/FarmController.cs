using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using MongoDB.Driver;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/farm")]
public class FarmController : ControllerBase
{
    private readonly MongoDbContext _mongoContext;

    public FarmController(MongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll([FromQuery] int userId)
    {
        var farms = await _mongoContext.Farms
            .Find(f => f.UserId == userId)
            .ToListAsync();
        return Ok(farms);
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int id)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        return Ok(farm);
    }

    //[HttpPost("crea")]
    //public async Task<IActionResult> Create([FromBody] AOFarm farm)
    //{
    //    // Initialize collections if they're null
    //    farm.Cultures ??= new List<AOCulture>();
    //    farm.Comments ??= new List<AOComment>();
    //    farm.Harvests ??= new List<AOHarvest>();
    //    farm.Metrics ??= new List<AOMetric>();

    //    await _mongoContext.Farms.InsertOneAsync(farm);
    //    return Ok(new { Id = farm.Id });
    //}

    [HttpPost("change")]
    public async Task<IActionResult> Change([FromQuery] int id, [FromBody] MongoFarm farmChanges)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        var update = Builders<MongoFarm>.Update
            .Set(f => f.Name, farmChanges.Name)
            .Set(f => f.UserId, farmChanges.UserId)
            .Set(f => f.SettlementId, farmChanges.SettlementId)
            .Set(f => f.Cultures, farmChanges.Cultures ?? farm.Cultures)
            .Set(f => f.Metrics, farmChanges.Metrics ?? farm.Metrics)
            .Set(f => f.Harvests, farmChanges.Harvests ?? farm.Harvests);

        await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            update
        );

        return Ok();
    }

    [HttpPost("addprop")]
    public async Task<IActionResult> AddProperty([FromQuery] int id, [FromBody] Dictionary<string, object> properties)
    {
        var update = Builders<MongoFarm>.Update;
        var updates = new List<UpdateDefinition<MongoFarm>>();

        foreach (var prop in properties)
        {
            updates.Add(update.Set(prop.Key, prop.Value));
        }

        if (updates.Count == 0)
            return BadRequest("No properties to add");

        var combinedUpdate = update.Combine(updates);
        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            combinedUpdate
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found or no changes made");

        return Ok();
    }

    [HttpPost("deleteprop")]
    public async Task<IActionResult> DeleteProperty([FromQuery] int id, [FromBody] List<string> propertyNames)
    {
        var protectedProps = new[] { "id", "settlementid", "cultures", "userId", "name", "metrics", "harvests", "comments" };
        var invalidProps = propertyNames.Select(x => x.ToLower()).Intersect(protectedProps).ToList();

        if (invalidProps.Any())
            return BadRequest($"Cannot delete protected properties: {string.Join(", ", invalidProps)}");

        var update = Builders<MongoFarm>.Update;
        var updates = propertyNames.Select(prop => update.Unset(prop));
        var combinedUpdate = update.Combine(updates);

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == id,
            combinedUpdate
        );

        if (result.ModifiedCount == 0)
            return NotFound("Farm not found or no changes made");

        return Ok();
    }

    [HttpGet("bysettlement")]
    public async Task<IActionResult> GetBySettlement([FromQuery] int settlementId)
    {
        var farms = await _mongoContext.Farms
            .Find(f => f.SettlementId == settlementId)
            .ToListAsync();
        return Ok(farms);
    }


    
}
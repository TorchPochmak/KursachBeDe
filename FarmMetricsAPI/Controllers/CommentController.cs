using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using MongoDB.Driver;

namespace FarmMetricsAPI.Controllers;

[ApiController]
[Route("api/comment")]
public class CommentController : ControllerBase
{
    private readonly MongoDbContext _mongoContext;

    public CommentController(MongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    [HttpGet("{farmId}/getall")]
    public async Task<IActionResult> GetAll(int farmId)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == farmId)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        // Return comments sorted by date in descending order (newest first)
        var sortedComments = farm.Comments
            .OrderByDescending(c => c.Date)
            .ToList();

        return Ok(sortedComments);
    }

    [HttpPost("{farmId}/create")]
    public async Task<IActionResult> Create(int farmId, [FromBody] MongoComment comment)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == farmId)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        // Set the current date
        comment.Date = DateTime.UtcNow;

        // Add the comment to the farm's comments list
        var update = Builders<MongoFarm>.Update.Push(f => f.Comments, comment);
        await _mongoContext.Farms.UpdateOneAsync(f => f.Id == farmId, update);

        return Ok(comment);
    }

    [HttpDelete("{farmId}/delete/{commentId}")]
    public async Task<IActionResult> Delete(int farmId, int commentId)
    {
        var farm = await _mongoContext.Farms
            .Find(f => f.Id == farmId)
            .FirstOrDefaultAsync();

        if (farm == null)
            return NotFound("Farm not found");

        // Remove the comment from the array using pull operator
        var update = Builders<MongoFarm>.Update.PullFilter(
            f => f.Comments,
            c => c.Id == commentId
        );

        var result = await _mongoContext.Farms.UpdateOneAsync(
            f => f.Id == farmId,
            update
        );

        if (result.ModifiedCount == 0)
            return NotFound("Comment not found");

        return Ok();
    }
} 
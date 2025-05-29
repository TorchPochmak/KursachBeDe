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
} 
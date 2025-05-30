using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Data.Redis;
using FarmMetricsAPI.Jobs;
using Quartz;
using Swashbuckle.AspNetCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbConfig"));
builder.Services.AddSingleton<MongoDbContext>();


builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisConfig"));
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConfig = builder.Configuration.GetSection("RedisConfig");
    options.Configuration = redisConfig["ConnectionString"];
    options.InstanceName = redisConfig["InstanceName"];
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("RedisConfig");
    return ConnectionMultiplexer.Connect(redisConfig["ConnectionString"]);
});
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbConfig = builder.Configuration.GetSection("DatabaseConfig");
    var connectionString = $"Host={dbConfig["Host"]};Port={dbConfig["Port"]};Database={dbConfig["Database"]};Username={dbConfig["Username"]};Password={dbConfig["Password"]}";
    options.UseNpgsql(connectionString);
});


builder.Services.AddQuartz(q =>
{

    var addMetricJobKey = new JobKey("AddMetricDataJob");
    q.AddJob<AddMetricDataJob>(opts => opts.WithIdentity(addMetricJobKey));
    q.AddTrigger(opts => opts
        .ForJob(addMetricJobKey)
        .WithIdentity("AddMetricDataJob-trigger")
        .WithCronSchedule("0 0 * * * ?")); 

    var cleanerJobKey = new JobKey("MetricDataCleanerJob");
    q.AddJob<MetricDataCleanerJob>(opts => opts.WithIdentity(cleanerJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanerJobKey)
        .WithIdentity("MetricDataCleanerJob-trigger")
        .WithCronSchedule("0 0 0 * * ?")); 

    var cacheCleanerJobKey = new JobKey("CacheCleanerJob");
    q.AddJob<CacheCleanerJob>(opts => opts.WithIdentity(cacheCleanerJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cacheCleanerJobKey)
        .WithIdentity("CacheCleanerJob-trigger")
        .WithCronSchedule("0 0 0 * * ?")); 
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddControllers();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Farm Metrics API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (await context.Database.CanConnectAsync())
        {
            Console.WriteLine("Подключение к базе данных прошло успешно!");
        }
        else
        {
            Console.WriteLine("Не удалось подключиться к базе данных.");
        }
        context.SeedTestData();
        Console.WriteLine("Test data seeded successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Исключение при подключении к базе данных: {ex.Message}");
    }
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

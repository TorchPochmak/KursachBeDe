using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models;
using FarmMetricsAPI.Models.Mongo;
using MongoDB.Driver;
using FarmMetricsAPI.Models.Postgres;

namespace FarmMetricsAPI.Data;

public static class TestData
{
    public static void SeedData(AppDbContext context, MongoDbContext mongoContext)
    {
        // Clear existing data
        mongoContext.Farms.DeleteMany(Builders<MongoFarm>.Filter.Empty);

        // Add roles
        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };
        context.Roles.AddRange(adminRole, userRole);
        context.SaveChanges();

        // Add settlements
        var settlement1 = new Settlement { Name = "Farm Settlement 1" };
        var settlement2 = new Settlement { Name = "Farm Settlement 2" };
        context.Settlements.AddRange(settlement1, settlement2);
        context.SaveChanges();

        // Add users
        var admin = new User 
        { 
            Name = "admin",
            Email = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            RoleId = adminRole.Id,
            SettlementId = settlement1.Id
        };
        var user = new User 
        {
            Name = "user",
            Email = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
            RoleId = userRole.Id,
            SettlementId = settlement2.Id
        };
        context.Users.AddRange(admin, user);
        context.SaveChanges();

        // Add metrics
        var temperatureMetric = new Metric { Name = "Temperature" };
        var humidityMetric = new Metric { Name = "Humidity" };
        var pressureMetric = new Metric { Name = "Pressure" };
        context.Metrics.AddRange(temperatureMetric, humidityMetric, pressureMetric);
        context.SaveChanges();

        // Add devices
        var device1 = new SettleMetricDevice 
        { 
            MetricId = temperatureMetric.Id,
            SettlementId = settlement1.Id,
            RegisteredAt = DateTime.UtcNow
        };
        var device2 = new SettleMetricDevice 
        { 
            MetricId = humidityMetric.Id,
            SettlementId = settlement1.Id,
            RegisteredAt = DateTime.UtcNow
        };
        var device3 = new SettleMetricDevice 
        { 
            MetricId = pressureMetric.Id,
            SettlementId = settlement2.Id,
            RegisteredAt = DateTime.UtcNow
        };
        context.SettleMetricDevices.AddRange(device1, device2, device3);
        context.SaveChanges();

        // Add metric data
        var random = new Random();
        var startDate = DateTime.UtcNow.AddDays(-7);
        for (int i = 0; i < 168; i++) // 7 days * 24 hours
        {
            var metricData1 = new MetricData 
            { 
                SettleMetricDeviceId = device1.Id,
                RegisteredAt = startDate.AddHours(i),
                MetricValue = random.NextDouble() * 30 + 10 // Temperature between 10-40
            };
            var metricData2 = new MetricData 
            { 
                SettleMetricDeviceId = device2.Id,
                RegisteredAt = startDate.AddHours(i),
                MetricValue = random.NextDouble() * 50 + 30 // Humidity between 30-80
            };
            var metricData3 = new MetricData 
            { 
                SettleMetricDeviceId = device3.Id,
                RegisteredAt = startDate.AddHours(i),
                MetricValue = random.NextDouble() * 20 + 990 // Pressure between 990-1010
            };
            context.MetricData.AddRange(metricData1, metricData2, metricData3);
        }
        context.SaveChanges();

        // Add MongoDB farms
        //var farm1 = new Farm
        //{
        //    Name = "Test Farm 1",
        //    UserId = admin.Id,
        //    SettlementId = settlement1.Id,
        //    Cultures = new List<Culture>
        //    {
        //        new Culture { Name = "Wheat", SquareMeters = 1000 },
        //        new Culture { Name = "Corn", SquareMeters = 2000 }
        //    },
        //    Metrics = new List<Metric>
        //    {
        //        new Metric { Name = "Temperature", Value = 25.5 },
        //        new Metric { Name = "Humidity", Value = 65.0 }
        //    },
        //    Comments = new List<Comment>
        //    {
        //        new Comment { Date = DateTime.UtcNow.AddDays(-2), Info = "Wheat growing well" },
        //        new Comment { Date = DateTime.UtcNow.AddDays(-1), Info = "Corn needs irrigation" }
        //    },
        //    Harvests = new List<Harvest>
        //    {
        //        new Harvest 
        //        { 
        //            RegisteredAt = DateTime.UtcNow.AddMonths(-1),
        //            Name = "Spring Wheat",
        //            Info = "Good yield"
        //        }
        //    }
        //};

        //var farm2 = new Farm
        //{
        //    Name = "Test Farm 2",
        //    UserId = user.Id,
        //    SettlementId = settlement2.Id,
        //    Cultures = new List<Culture>
        //    {
        //        new Culture { Name = "Potatoes", SquareMeters = 500 },
        //        new Culture { Name = "Carrots", SquareMeters = 300 }
        //    },
        //    Metrics = new List<Metric>
        //    {
        //        new Metric { Name = "Pressure", Value = 1005.0 }
        //    },
        //    Comments = new List<Comment>
        //    {
        //        new Comment { Date = DateTime.UtcNow.AddDays(-3), Info = "Started planting potatoes" }
        //    },
        //    Harvests = new List<Harvest>
        //    {
        //        new Harvest 
        //        { 
        //            RegisteredAt = DateTime.UtcNow.AddMonths(-2),
        //            Name = "Winter Carrots",
        //            Info = "Average yield"
        //        }
        //    }
        //};

        //mongoContext.Farms.InsertMany(new[] { farm1, farm2 });
    }
}
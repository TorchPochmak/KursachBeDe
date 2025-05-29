using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models;
using FarmMetricsAPI.Models.Mongo;
using MongoDB.Driver;
using FarmMetricsAPI.Models.Postgres;
using Microsoft.EntityFrameworkCore;

namespace FarmMetricsAPI.Data;

public static class TestData
{
    public static void SeedData(AppDbContext context, MongoDbContext mongoContext)
    {
        Console.WriteLine("Starting TestData.SeedData...");
        
        // Clear existing data
        try 
        {
            Console.WriteLine("Clearing existing MongoDB farms...");
            mongoContext.Farms.DeleteMany(Builders<MongoFarm>.Filter.Empty);
            Console.WriteLine("Successfully cleared MongoDB farms.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing MongoDB farms: {ex.Message}");
            throw;
        }

        if (!context.Settlements.Any())
        {
            Console.WriteLine("Seeding settlements...");
            var citiesPath = Path.Combine("Data", "cities.txt");
            if (File.Exists(citiesPath))
            {
                var cities = File.ReadAllLines(citiesPath);
                foreach (var city in cities)
                {
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        var parts = city.Split('\t');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int populationInThousands))
                        {
                            context.Settlements.Add(new Settlement 
                            { 
                                Name = parts[0].Trim(),
                                Population = populationInThousands * 1000
                            });
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        // Add roles
        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };
        context.Roles.AddRange(adminRole, userRole);
        context.SaveChanges();

        Random random = new Random();
        // Get random settlements for users
        var allSettlements = context.Settlements.ToList();
        var randomSettlements = allSettlements
            .OrderBy(x => random.Next())
            .Take(5)
            .ToList();

        // Add users
        var users = new List<User>
        {
            new User 
            { 
                Name = "Администратор",
                Email = "admin@admin.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                RoleId = adminRole.Id,
                SettlementId = randomSettlements[0].Id
            },
            new User 
            {
                Name = "Иван Петров",
                Email = "ivan@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
                RoleId = userRole.Id,
                SettlementId = randomSettlements[1].Id
            },
            new User 
            {
                Name = "Елена Смирнова",
                Email = "elena@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
                RoleId = userRole.Id,
                SettlementId = randomSettlements[2].Id
            },
            new User 
            {
                Name = "Алексей Иванов",
                Email = "alexey@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
                RoleId = userRole.Id,
                SettlementId = randomSettlements[3].Id
            },
            new User 
            {
                Name = "Мария Козлова",
                Email = "maria@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
                RoleId = userRole.Id,
                SettlementId = randomSettlements[4].Id
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // Add metrics
        var metrics = new List<Metric>
        {
            new Metric { Name = "Температура воздуха (°C)", MinValue = -40, MaxValue = 50 },
            new Metric { Name = "Влажность воздуха (%)", MinValue = 0, MaxValue = 100 },
            new Metric { Name = "Влажность почвы (%)", MinValue = 0, MaxValue = 100 },
            new Metric { Name = "Атмосферное давление (мм рт.ст.)", MinValue = 700, MaxValue = 800 },
            new Metric { Name = "Уровень pH почвы", MinValue = 0, MaxValue = 14 },
            new Metric { Name = "Освещенность (люкс)", MinValue = 0, MaxValue = 100000 },
            new Metric { Name = "Скорость ветра (м/с)", MinValue = 0, MaxValue = 50 },
            new Metric { Name = "Количество осадков (мм/сут)", MinValue = 0, MaxValue = 100 },
            new Metric { Name = "Содержание CO2 (ppm)", MinValue = 0, MaxValue = 5000 },
            new Metric { Name = "Уровень УФ-излучения (УФ-индекс)", MinValue = 0, MaxValue = 11 },
            new Metric { Name = "Температура почвы (°C)", MinValue = -20, MaxValue = 40 },
            new Metric { Name = "Электропроводность почвы (мСм/см)", MinValue = 0, MaxValue = 20 },
            new Metric { Name = "Содержание азота (мг/кг)", MinValue = 0, MaxValue = 300 },
            new Metric { Name = "Содержание фосфора (мг/кг)", MinValue = 0, MaxValue = 200 },
            new Metric { Name = "Содержание калия (мг/кг)", MinValue = 0, MaxValue = 500 }
        };
        context.Metrics.AddRange(metrics);
        context.SaveChanges();

        var settlements = context.Settlements.ToList();
        
        foreach (var settlement in settlements)
        {
            var devices = new List<SettleMetricDevice>
            {
                new SettleMetricDevice 
                { 
                    MetricId = metrics[0].Id, // Температура воздуха
                    SettlementId = settlement.Id,
                    RegisteredAt = DateTime.UtcNow
                },
                new SettleMetricDevice 
                { 
                    MetricId = metrics[1].Id, // Влажность воздуха
                    SettlementId = settlement.Id,
                    RegisteredAt = DateTime.UtcNow
                }
            };
            
            var additionalMetricsCount = random.Next(1, 6); // 1 to 5 additional metrics
            var availableMetrics = metrics.Skip(2).ToList(); // Skip temperature and humidity
            
            var selectedMetrics = availableMetrics
                .OrderBy(x => random.NextDouble())
                .Take(additionalMetricsCount)
                .ToList();

            foreach (var metric in selectedMetrics)
            {
                devices.Add(new SettleMetricDevice
                {
                    MetricId = metric.Id,
                    SettlementId = settlement.Id,
                    RegisteredAt = DateTime.UtcNow
                });
            }

            context.SettleMetricDevices.AddRange(devices);
            context.SaveChanges(); // Save devices first to get their IDs

            // Add metric data for each device
            var startDate = DateTime.UtcNow.Date.AddDays(-7).AddHours(DateTime.UtcNow.Hour);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0, DateTimeKind.Utc); // Reset minutes and seconds to 00:00

            foreach (var device in devices)
            {
                var metric = metrics.First(m => m.Id == device.MetricId);
                for (int i = 0; i < 168; i++) // 7 days * 24 hours
                {
                    context.MetricData.Add(new MetricData 
                    { 
                        SettleMetricDeviceId = device.Id,
                        RegisteredAt = startDate.AddHours(i), // This will maintain 00:00 minutes and seconds
                        MetricValue = random.NextDouble() * (metric.MaxValue - metric.MinValue) + metric.MinValue
                    });
                }
            }
        }
        context.SaveChanges();

        // Add MongoDB farms for each user
        var culturePool = new List<(string Name, int MinSquareMeters, int MaxSquareMeters)>
        {
            ("Пшеница озимая", 1000, 10000),
            ("Пшеница яровая", 1000, 8000),
            ("Кукуруза", 1000, 8000),
            ("Картофель", 500, 5000),
            ("Морковь", 500, 3000),
            ("Свекла сахарная", 500, 4000),
            ("Свекла кормовая", 500, 4000),
            ("Подсолнечник", 1000, 7000),
            ("Соя", 800, 6000),
            ("Рожь", 1000, 9000),
            ("Овес", 1000, 8000),
            ("Ячмень", 1000, 9000),
            ("Горох", 500, 3000),
            ("Рапс", 800, 5000),
            ("Гречиха", 700, 4000)
        };

        var harvestPool = new List<(string Name, string Info)>
        {
            ("Сбор пшеницы", "Отличный урожай, высокое качество зерна"),
            ("Уборка кукурузы", "Хороший урожай, оптимальная влажность"),
            ("Сбор картофеля", "Средний урожай, крупные клубни"),
            ("Уборка свеклы", "Высокая урожайность, хорошая сахаристость"),
            ("Сбор подсолнечника", "Хороший урожай, высокая масличность"),
            ("Уборка сои", "Урожайность выше среднего"),
            ("Сбор ржи", "Стандартный урожай, хорошее качество"),
            ("Уборка овса", "Урожай соответствует ожиданиям"),
            ("Сбор ячменя", "Высокий урожай, отличное качество"),
            ("Уборка гороха", "Средний урожай, хорошее качество"),
            ("Сбор рапса", "Хорошая урожайность, высокая масличность"),
            ("Уборка гречихи", "Стабильный урожай")
        };

        var commentPool = new List<string>
        {
            "Проведена диагностика системы орошения, все работает исправно",
            "Обнаружены признаки заболевания растений, начато применение фунгицидов",
            "Установлены новые метеодатчики для мониторинга климата",
            "Завершена подготовка почвы к посевной, внесены основные удобрения",
            "Проведена инвентаризация техники, требуется ремонт трактора",
            "Начато строительство нового складского помещения",
            "Заключен договор на поставку семян на следующий сезон",
            "Успешно пройдена сертификация органического производства",
            "Внедрена новая система учета расхода воды",
            "Проведено обучение персонала по работе с новой техникой"
        };

        foreach (var user in users)
        {
            Console.WriteLine($"\nProcessing user: {user.Name}");
            // Get available metrics for this settlement
            var settlementDevices = context.SettleMetricDevices
                .Where(d => d.SettlementId == user.SettlementId)
                .Include(d => d.Metric)
                .ToList();

            var availableMetrics = settlementDevices
                .Select(d => d.Metric)
                .ToList();

            if (!availableMetrics.Any())
            {
                Console.WriteLine($"No metrics available for user {user.Name}, skipping...");
                continue;
            }

            try
            {
                var farms = new List<MongoFarm>
                {
                    new MongoFarm
                    {
                        Name = $"Ферма №1 - {user.Name}",
                        UserId = user.Id,
                        SettlementId = user.SettlementId.Value,
                        Cultures = culturePool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(3, 6))
                            .Select(c => new MongoCulture 
                            { 
                                Name = c.Name, 
                                SquareMeters = random.Next(c.MinSquareMeters, c.MaxSquareMeters + 1)
                            })
                            .ToList(),
                        Metrics = availableMetrics
                            .OrderBy(x => random.NextDouble())
                            .Take(Math.Min(3, availableMetrics.Count()))
                            .Select(m => new MongoMetric 
                            { 
                                Name = m.Name,
                                Value = random.NextDouble() * (m.MaxValue - m.MinValue) + m.MinValue,
                                DeviceId = settlementDevices.First(d => d.Metric.Name == m.Name).Id
                            })
                            .ToList(),
                        Comments = commentPool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(3, 6))
                            .Select(c => new MongoComment 
                            { 
                                Date = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                                Info = c 
                            })
                            .OrderByDescending(c => c.Date)
                            .ToList(),
                        Harvests = harvestPool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(2, 4))
                            .Select(h => new MongoHarvest 
                            { 
                                RegisteredAt = DateTime.UtcNow.AddMonths(random.Next(-6, -1)),
                                Name = h.Name,
                                Info = h.Info
                            })
                            .ToList()
                    },
                    new MongoFarm
                    {
                        Name = $"Ферма №2 - {user.Name}",
                        UserId = user.Id,
                        SettlementId = user.SettlementId.Value,
                        Cultures = culturePool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(3, 6))
                            .Select(c => new MongoCulture 
                            { 
                                Name = c.Name, 
                                SquareMeters = random.Next(c.MinSquareMeters, c.MaxSquareMeters + 1)
                            })
                            .ToList(),
                        Metrics = availableMetrics
                            .OrderBy(x => random.NextDouble())
                            .Take(Math.Min(4, availableMetrics.Count()))
                            .Select(m => new MongoMetric 
                            { 
                                Name = m.Name,
                                Value = random.NextDouble() * (m.MaxValue - m.MinValue) + m.MinValue,
                                DeviceId = settlementDevices.First(d => d.Metric.Name == m.Name).Id
                            })
                            .ToList(),
                        Comments = commentPool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(3, 6))
                            .Select(c => new MongoComment 
                            { 
                                Date = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                                Info = c 
                            })
                            .OrderByDescending(c => c.Date)
                            .ToList(),
                        Harvests = harvestPool
                            .OrderBy(x => random.NextDouble())
                            .Take(random.Next(2, 4))
                            .Select(h => new MongoHarvest 
                            { 
                                RegisteredAt = DateTime.UtcNow.AddMonths(random.Next(-6, -1)),
                                Name = h.Name,
                                Info = h.Info
                            })
                            .ToList()
                    }
                };

                Console.WriteLine($"Created farms for user {user.Name}:");
                foreach (var farm in farms)
                {
                    Console.WriteLine($"- Farm: {farm.Name}");
                    Console.WriteLine($"- Farm: {farm.Id}");
                    Console.WriteLine($"  Cultures: {farm.Cultures.Count}");
                    Console.WriteLine($"  Metrics: {farm.Metrics.Count}");
                    Console.WriteLine($"  Comments: {farm.Comments.Count}");
                    Console.WriteLine($"  Harvests: {farm.Harvests.Count}");
                }

                try
                {
                    mongoContext.Farms.InsertMany(farms);
                    Console.WriteLine($"Successfully inserted farms for user {user.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting farms for user {user.Name}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating farms for user {user.Name}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        Console.WriteLine("\nFinished seeding all farms.");
    }
}
using Microsoft.EntityFrameworkCore;
using FarmMetricsAPI.Controllers;
using MongoDB.Driver;
using FarmMetricsAPI.Data.MongoDb;
using FarmMetricsAPI.Models.Mongo;
using FarmMetricsAPI.Models.Postgres;

namespace FarmMetricsAPI.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricData> MetricData { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<SettleMetricDevice> SettleMetricDevices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public void SeedTestData()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            var mongoContext = (MongoDbContext)_serviceProvider.GetService(typeof(MongoDbContext));
            if (mongoContext != null)
            {
                mongoContext.Farms.DeleteMany(Builders<MongoFarm>.Filter.Empty);
                TestData.SeedData(this, mongoContext);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<MetricData>()
                .HasOne(md => md.Device)
                .WithMany(d => d.MetricData)
                .HasForeignKey(md => md.SettleMetricDeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<User>()
                .HasOne(u => u.Settlement)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SettlementId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<SettleMetricDevice>()
                .HasOne(smd => smd.Metric)
                .WithMany(m => m.SettleMetricDevices)
                .HasForeignKey(smd => smd.MetricId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<SettleMetricDevice>()
                .HasOne(smd => smd.Settlement)
                .WithMany(s => s.SettleMetricDevices)
                .HasForeignKey(smd => smd.SettlementId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}

using AnalyticsService.Models;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Data
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        public DbSet<OccupancyLog> OccupancyLogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<OccupancyLog>();
            entity.ToTable("OccupancyLogs");
            entity.HasKey(log => log.LogId);
            entity.Property(log => log.VehicleType).HasMaxLength(20).IsRequired();
            entity.Property(log => log.Timestamp).HasDefaultValueSql("NOW()");
            entity.Property(log => log.OccupancyRate).HasColumnType("double precision");

            entity.HasIndex(log => log.LotId);
            entity.HasIndex(log => log.SpotId);
            entity.HasIndex(log => log.Timestamp);
            entity.HasIndex(log => log.VehicleType);
            entity.HasIndex(log => new { log.LotId, log.Timestamp });
        }
    }
}

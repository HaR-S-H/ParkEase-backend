using Microsoft.EntityFrameworkCore;
using ParkingSpotService.Models;

namespace ParkingSpotService.Data
{
    public class ParkingSpotDbContext : DbContext
    {
        public ParkingSpotDbContext(DbContextOptions<ParkingSpotDbContext> options) : base(options)
        {
        }

        public DbSet<ParkingSpot> ParkingSpots { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<ParkingSpot>();

            entity.ToTable("ParkingSpots");
            entity.HasKey(spot => spot.SpotId);
            entity.Property(spot => spot.SpotNumber).HasMaxLength(50).IsRequired();
            entity.Property(spot => spot.SpotType).HasConversion<string>().HasMaxLength(20);
            entity.Property(spot => spot.VehicleType).HasConversion<string>().HasMaxLength(20);
            entity.Property(spot => spot.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(spot => spot.PricePerHour).HasColumnType("double precision");
            entity.Property(spot => spot.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(spot => spot.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(spot => new { spot.LotId, spot.SpotNumber }).IsUnique();
            entity.HasIndex(spot => new { spot.LotId, spot.Status });
            entity.HasIndex(spot => new { spot.LotId, spot.SpotType });
            entity.HasIndex(spot => new { spot.LotId, spot.VehicleType });
            entity.HasIndex(spot => spot.IsEVCharging);
            entity.HasIndex(spot => spot.IsHandicapped);
        }
    }
}
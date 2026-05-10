using Microsoft.EntityFrameworkCore;
using VehicleService.Models;

namespace VehicleService.Data
{
    public class VehicleDbContext : DbContext
    {
        public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<Vehicle>();

            entity.ToTable("Vehicles");
            entity.HasKey(vehicle => vehicle.VehicleId);
            entity.Property(vehicle => vehicle.LicensePlate).HasMaxLength(30).IsRequired();
            entity.Property(vehicle => vehicle.Make).HasMaxLength(80).IsRequired();
            entity.Property(vehicle => vehicle.Model).HasMaxLength(80).IsRequired();
            entity.Property(vehicle => vehicle.Color).HasMaxLength(40).IsRequired();
            entity.Property(vehicle => vehicle.VehicleType).HasMaxLength(20).IsRequired();
            entity.Property(vehicle => vehicle.RegisteredAt).HasColumnType("date");
            entity.Property(vehicle => vehicle.IsActive).HasDefaultValue(true);

            entity.HasIndex(vehicle => vehicle.OwnerId);
            entity.HasIndex(vehicle => vehicle.VehicleType);
            entity.HasIndex(vehicle => vehicle.IsEV);
            entity.HasIndex(vehicle => new { vehicle.OwnerId, vehicle.LicensePlate }).IsUnique();
        }
    }
}

using ParkingLotService.Models;
using Microsoft.EntityFrameworkCore;

namespace ParkingLotService.Data
{
    public class ParkingLotDbContext : DbContext
    {
        public ParkingLotDbContext(DbContextOptions<ParkingLotDbContext> options) : base(options)
        {
        }

        public DbSet<ParkingLot> ParkingLots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingLot>(entity =>
            {
                entity.HasIndex(lot => lot.City);
                entity.HasIndex(lot => lot.ManagerId);
                entity.HasIndex(lot => lot.IsApproved);

                entity.Property(lot => lot.Name).HasMaxLength(200);
                entity.Property(lot => lot.Address).HasMaxLength(300);
                entity.Property(lot => lot.City).HasMaxLength(120);
                entity.Property(lot => lot.ImageUrl).HasMaxLength(1000);
                entity.Property(lot => lot.AvailableSpots).IsConcurrencyToken();
            });
        }
    }
}
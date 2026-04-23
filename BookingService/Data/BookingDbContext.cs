using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<Booking>();

            entity.ToTable("Bookings");
            entity.HasKey(booking => booking.BookingId);
            entity.Property(booking => booking.VehiclePlate).HasMaxLength(30).IsRequired();
            entity.Property(booking => booking.VehicleType).HasMaxLength(20).IsRequired();
            entity.Property(booking => booking.BookingType).HasConversion<string>().HasMaxLength(20);
            entity.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(booking => booking.TotalAmount).HasColumnType("double precision");
            entity.Property(booking => booking.CheckInTime).IsRequired(false);
            entity.Property(booking => booking.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(booking => booking.UserId);
            entity.HasIndex(booking => booking.LotId);
            entity.HasIndex(booking => booking.SpotId);
            entity.HasIndex(booking => booking.Status);
            entity.HasIndex(booking => booking.VehiclePlate);
            entity.HasIndex(booking => booking.CheckInTime);
            entity.HasIndex(booking => new { booking.LotId, booking.Status });
            entity.HasIndex(booking => new { booking.BookingType, booking.Status, booking.CreatedAt });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<Payment>();

            entity.ToTable("Payments");
            entity.HasKey(payment => payment.PaymentId);
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(payment => payment.Mode).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(payment => payment.TransactionId).HasMaxLength(100).IsRequired();
            entity.Property(payment => payment.Currency).HasMaxLength(8).IsRequired();
            entity.Property(payment => payment.Description).HasMaxLength(300);
            entity.Property(payment => payment.Amount).HasColumnType("double precision");
            entity.Property(payment => payment.PaidAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(payment => payment.BookingId).IsUnique();
            entity.HasIndex(payment => payment.UserId);
            entity.HasIndex(payment => payment.LotId);
            entity.HasIndex(payment => payment.Status);
            entity.HasIndex(payment => payment.TransactionId).IsUnique();
            entity.HasIndex(payment => payment.PaidAt);
        }
    }
}

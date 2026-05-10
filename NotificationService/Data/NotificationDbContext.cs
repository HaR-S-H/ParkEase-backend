using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<Notification>();
            entity.ToTable("Notifications");
            entity.HasKey(notification => notification.NotificationId);
            entity.Property(notification => notification.Type).HasMaxLength(30).IsRequired();
            entity.Property(notification => notification.Title).HasMaxLength(150).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(1000).IsRequired();
            entity.Property(notification => notification.Channel).HasMaxLength(20).IsRequired();
            entity.Property(notification => notification.RelatedType).HasMaxLength(50);
            entity.Property(notification => notification.SentAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(notification => notification.RecipientId);
            entity.HasIndex(notification => notification.IsRead);
            entity.HasIndex(notification => notification.Type);
            entity.HasIndex(notification => new { notification.RecipientId, notification.IsRead });
            entity.HasIndex(notification => new { notification.RelatedType, notification.RelatedId });
        }
    }
}

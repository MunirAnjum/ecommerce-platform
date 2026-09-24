using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Persistences
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Recipient).IsRequired().HasMaxLength(320);

                entity.Property(x => x.Subject).IsRequired().HasMaxLength(200);

                entity.Property(x => x.Message).IsRequired();

                entity.Property(x => x.Type).IsRequired();

                entity.Property(x => x.IsSent).IsRequired();

                entity.HasIndex(x => x.UserId);
            });
        }
    }
}

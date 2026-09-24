using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Outbox;
using PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Persistences
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {  
        }

        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.OrderId).IsUnique();

                entity.Property(x => x.Amount).HasPrecision(18, 2);

                entity.Property(x => x.Method).IsRequired();

                entity.Property(x => x.Status).IsRequired();

                entity.Property(x => x.TransactionId).HasMaxLength(200);
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(x => x.EventType)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.RoutingKey)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Payload)
                    .IsRequired();

                entity.Property(x => x.Error)
                    .HasMaxLength(2000);

                entity.HasIndex(x => x.ProcessedOnUtc);
            });
        }
    }
}

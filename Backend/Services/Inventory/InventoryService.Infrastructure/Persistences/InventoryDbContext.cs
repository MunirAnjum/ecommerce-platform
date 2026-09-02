using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Persistences;
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ProductId).IsUnique();

            entity.Property(x => x.AvailableQuantity).IsRequired();

            entity.Property(x => x.ReservedQuantity).IsRequired();

            entity.Property(x => x.CreatedAt).IsRequired();
        });
    }
}


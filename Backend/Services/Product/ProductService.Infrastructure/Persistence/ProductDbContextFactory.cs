using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5433;Database=ecommerceproductdb;Username=postgres;Password=ecommerce@123");

        return new ProductDbContext(optionsBuilder.Options);
    }
}

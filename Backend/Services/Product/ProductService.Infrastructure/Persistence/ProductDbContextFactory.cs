/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Persistence;
public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<ProductDbContext>();

        optionBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=EcommerceProductDb;Username=postgres;Password=ecommerce@123");

        return new ProductDbContext(optionBuilder.Options);
    }
}*/
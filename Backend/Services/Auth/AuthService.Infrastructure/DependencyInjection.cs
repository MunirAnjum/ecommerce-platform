using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AuthDbContext>(option =>
                option.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")));
            // UserRepository
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            //PasswordHash
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            //JWT
            services.AddScoped<IJwtService, JwtService>();
            return services;

        }
    }
}

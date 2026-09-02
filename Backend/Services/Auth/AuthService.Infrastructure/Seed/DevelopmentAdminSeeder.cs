using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Seed;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(
        AuthDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        var adminExists = await context.Users
            .AnyAsync(x => x.Role == UserRole.Admin);

        if (adminExists)
        {
            return;
        }

        var adminSection =
            configuration.GetSection("DevelopmentAdmin");

        var firstName = adminSection["FirstName"];
        var lastName = adminSection["LastName"];
        var email = adminSection["Email"];
        var password = adminSection["Password"];

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "DevelopmentAdmin configuration is incomplete.");
        }

        var existingUser = await context.Users
            .FirstOrDefaultAsync(
                x => x.Email == email.ToLower());

        if (existingUser is not null)
        {
            return;
        }

        var passwordHash = passwordHasher.Hash(password);

        var admin = new User(
            firstName,
            lastName,
            email.ToLowerInvariant(),
            passwordHash);

        admin.MakeAdmin();

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
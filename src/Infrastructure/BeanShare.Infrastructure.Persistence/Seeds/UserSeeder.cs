using BeanShare.Domain.Entities;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class UserSeeder : IDataSeeder
{
    public int Order => 1; // Users must be seeded first

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if users already exist
        if (await context.Set<User>().AnyAsync(cancellationToken))
            return;

        var users = GetSeedUsers();
        await context.Set<User>().AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<User> GetSeedUsers()
    {
        return new List<User>
        {
            new User
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Email = "john.smith@beanshare.com",
                Name = "John Smith",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new User
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Email = "sarah.johnson@beanshare.com",
                Name = "Sarah Johnson",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5),
                UpdatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new User
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Email = "mike.wilson@beanshare.com",
                Name = "Mike Wilson",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Email = "emma.davis@beanshare.com",
                Name = "Emma Davis",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Email = "alex.brown@beanshare.com",
                Name = "Alex Brown",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new User
            {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                Email = "lisa.martinez@beanshare.com",
                Name = "Lisa Martinez",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new User
            {
                Id = new Guid("77777777-7777-7777-7777-777777777777"),
                Email = "david.garcia@beanshare.com",
                Name = "David Garcia",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EmailVerified = false, // Unverified user for testing
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Id = new Guid("88888888-8888-8888-8888-888888888888"),
                Email = "test@beanshare.com",
                Name = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow
            }
        };
    }
}
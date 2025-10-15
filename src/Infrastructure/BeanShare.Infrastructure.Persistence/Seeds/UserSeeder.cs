using BeanShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class UserSeeder : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<User>().AnyAsync(cancellationToken))
            return;

        var users = GetSeedUsers();
        await context.Set<User>().AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<User> GetSeedUsers()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var testPasswordHash = BCrypt.Net.BCrypt.HashPassword("test123");

        return new List<User>
        {
            User.CreateWithPassword("john.smith@beanshare.com", "John Smith", passwordHash),
            User.CreateWithPassword("sarah.johnson@beanshare.com", "Sarah Johnson", passwordHash),
            User.CreateWithPassword("mike.wilson@beanshare.com", "Mike Wilson", passwordHash),
            User.CreateWithPassword("emma.davis@beanshare.com", "Emma Davis", passwordHash),
            User.CreateWithPassword("alex.brown@beanshare.com", "Alex Brown", passwordHash),
            User.CreateWithPassword("lisa.martinez@beanshare.com", "Lisa Martinez", passwordHash),
            User.CreateWithPassword("david.garcia@beanshare.com", "David Garcia", passwordHash),
            User.CreateWithPassword("test@beanshare.com", "Test User", testPasswordHash)
        };
    }
}

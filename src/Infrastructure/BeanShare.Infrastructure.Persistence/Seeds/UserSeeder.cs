using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
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
        var now = DateTime.UtcNow;

        var adminUser = User.CreateWithIdAndPassword(new Guid("11111111-1111-1111-1111-111111111111"), "arnzrk@gmail.com", "John Smith", passwordHash, now);
        adminUser.SetSystemRole(BeanShare.Domain.Enums.SystemRole.SystemAdmin);

        return new List<User>
        {
            adminUser,
            User.CreateWithIdAndPassword(new Guid("22222222-2222-2222-2222-222222222222"), "sarah.johnson@beanshare.com", "Sarah Johnson", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("33333333-3333-3333-3333-333333333333"), "mike.wilson@beanshare.com", "Mike Wilson", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("44444444-4444-4444-4444-444444444444"), "emma.davis@beanshare.com", "Emma Davis", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("55555555-5555-5555-5555-555555555555"), "alex.brown@beanshare.com", "Alex Brown", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("66666666-6666-6666-6666-666666666666"), "lisa.martinez@beanshare.com", "Lisa Martinez", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("77777777-7777-7777-7777-777777777777"), "david.garcia@beanshare.com", "David Garcia", passwordHash, now),
            User.CreateWithIdAndPassword(new Guid("88888888-8888-8888-8888-888888888888"), "test@beanshare.com", "Test User", testPasswordHash, now)
        };
    }
}

using BeanShare.Domain.Aggregates;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class SpaceSeeder : IDataSeeder
{
    public int Order => 2; // Spaces depend on Users

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if spaces already exist
        if (await context.Spaces.AnyAsync(cancellationToken))
            return;

        var spaces = GetSeedSpaces();
        await context.Spaces.AddRangeAsync(spaces, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<Space> GetSeedSpaces()
    {
        var spaces = new List<Space>();

        // Engineering Team Space
        var engineeringSpace = Space.Create(
            "Engineering Team",
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")) // John Smith as admin
        );
        engineeringSpace.Id = new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111"));
        engineeringSpace.InviteCode = "ENG2024";

        // Add members to Engineering Team
        engineeringSpace.AddMember(new UserId(new Guid("22222222-2222-2222-2222-222222222222")), "Member"); // Sarah
        engineeringSpace.AddMember(new UserId(new Guid("33333333-3333-3333-3333-333333333333")), "Member"); // Mike
        engineeringSpace.AddMember(new UserId(new Guid("44444444-4444-4444-4444-444444444444")), "Admin");  // Emma as admin
        engineeringSpace.AddMember(new UserId(new Guid("55555555-5555-5555-5555-555555555555")), "Member"); // Alex
        engineeringSpace.AddMember(new UserId(new Guid("88888888-8888-8888-8888-888888888888")), "Member"); // Test User

        spaces.Add(engineeringSpace);

        // Marketing Office Space
        var marketingSpace = Space.Create(
            "Marketing Office",
            new UserId(new Guid("22222222-2222-2222-2222-222222222222")) // Sarah as admin
        );
        marketingSpace.Id = new SpaceId(new Guid("bbbb2222-bbbb-2222-bbbb-222222222222"));
        marketingSpace.InviteCode = "MKT2024";

        // Add members to Marketing Office
        marketingSpace.AddMember(new UserId(new Guid("44444444-4444-4444-4444-444444444444")), "Member"); // Emma
        marketingSpace.AddMember(new UserId(new Guid("66666666-6666-6666-6666-666666666666")), "Admin");  // Lisa as admin
        marketingSpace.AddMember(new UserId(new Guid("77777777-7777-7777-7777-777777777777")), "Member"); // David

        spaces.Add(marketingSpace);

        // Remote Workers Hub
        var remoteSpace = Space.Create(
            "Remote Workers Hub",
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")) // Mike as admin
        );
        remoteSpace.Id = new SpaceId(new Guid("cccc3333-cccc-3333-cccc-333333333333"));
        remoteSpace.InviteCode = "REMOTE24";

        // Add members to Remote Workers Hub
        remoteSpace.AddMember(new UserId(new Guid("11111111-1111-1111-1111-111111111111")), "Member"); // John
        remoteSpace.AddMember(new UserId(new Guid("55555555-5555-5555-5555-555555555555")), "Member"); // Alex
        remoteSpace.AddMember(new UserId(new Guid("66666666-6666-6666-6666-666666666666")), "Member"); // Lisa
        remoteSpace.AddMember(new UserId(new Guid("88888888-8888-8888-8888-888888888888")), "Admin");  // Test User as admin

        spaces.Add(remoteSpace);

        // Startup Garage
        var startupSpace = Space.Create(
            "Startup Garage",
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")) // Alex as admin
        );
        startupSpace.Id = new SpaceId(new Guid("dddd4444-dddd-4444-dddd-444444444444"));
        startupSpace.InviteCode = "STARTUP1";

        // Smaller team
        startupSpace.AddMember(new UserId(new Guid("11111111-1111-1111-1111-111111111111")), "Member"); // John
        startupSpace.AddMember(new UserId(new Guid("33333333-3333-3333-3333-333333333333")), "Admin");  // Mike as admin

        spaces.Add(startupSpace);

        // Executive Lounge (exclusive)
        var executiveSpace = Space.Create(
            "Executive Lounge",
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")) // John as admin
        );
        executiveSpace.Id = new SpaceId(new Guid("eeee5555-eeee-5555-eeee-555555555555"));
        executiveSpace.InviteCode = "EXEC2024";

        // Only executives
        executiveSpace.AddMember(new UserId(new Guid("22222222-2222-2222-2222-222222222222")), "Admin"); // Sarah as admin
        executiveSpace.AddMember(new UserId(new Guid("44444444-4444-4444-4444-444444444444")), "Admin"); // Emma as admin

        spaces.Add(executiveSpace);

        return spaces;
    }
}
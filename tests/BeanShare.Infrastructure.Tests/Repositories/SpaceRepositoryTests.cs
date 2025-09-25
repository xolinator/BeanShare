using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using BeanShare.Infrastructure.Persistence.Repositories;
using BeanShare.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BeanShare.Infrastructure.Tests.Repositories;

public sealed class SpaceRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public SpaceRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task AddAsync_WithValidSpace_ShouldPersistToDatabase()
    {
        await using var context = await _databaseFixture.CreateDbContextAsync();
        var repository = new SpaceRepository(context);
        var clock = new TestClock();
        
        var spaceId = SpaceId.New();
        var creatorId = new UserId(Guid.NewGuid());
        var inviteCode = new InviteCode("CAFE23");
        var space = Space.Create(spaceId, "Test Coffee Space", creatorId, inviteCode, clock);

        await repository.AddAsync(space);
        await context.SaveChangesAsync();

        var specification = new SpaceByIdSpecification(spaceId);
        var retrieved = await repository.GetSingleBySpecAsync(specification);
        
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Coffee Space");
        retrieved.InviteCode.Should().Be(inviteCode);
        retrieved.Members.Should().HaveCount(1);
        retrieved.Members.First().UserId.Should().Be(creatorId);
    }

    [Fact]
    public async Task GetByInviteCodeAsync_WithValidCode_ShouldReturnSpace()
    {
        await using var context = await _databaseFixture.CreateDbContextAsync();
        var repository = new SpaceRepository(context);
        var clock = new TestClock();
        
        var spaceId = SpaceId.New();
        var creatorId = new UserId(Guid.NewGuid());
        var inviteCode = new InviteCode("BREW42");
        var space = Space.Create(spaceId, "Brew Space", creatorId, inviteCode, clock);

        await repository.AddAsync(space);
        await context.SaveChangesAsync();

        var specification = new SpaceByInviteCodeSpecification(inviteCode);
        var retrieved = await repository.GetSingleBySpecAsync(specification);
        
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Brew Space");
        retrieved.Id.Should().Be(spaceId);
    }

    [Fact]
    public async Task GetUserSpacesAsync_WithUserInMultipleSpaces_ShouldReturnAllUserSpaces()
    {
        await using var context = await _databaseFixture.CreateDbContextAsync();
        var repository = new SpaceRepository(context);
        var clock = new TestClock();
        
        var userId = new UserId(Guid.NewGuid());
        var creatorId = new UserId(Guid.NewGuid());
        
        var space1 = Space.Create(SpaceId.New(), "Space 1", userId, new InviteCode("BCDEFG"), clock);
        var space2 = Space.Create(SpaceId.New(), "Space 2", creatorId, new InviteCode("HJKLMN"), clock);
        space2.Join(userId, clock);

        await repository.AddAsync(space1);
        await repository.AddAsync(space2);
        await context.SaveChangesAsync();

        var specification = new SpacesWithUserMembershipSpecification(userId);
        var userSpaces = await repository.GetBySpecAsync(specification);
        
        userSpaces.Should().HaveCount(2);
        userSpaces.Should().Contain(s => s.Name == "Space 1");
        userSpaces.Should().Contain(s => s.Name == "Space 2");
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2025, 9, 14, 12, 0, 0, DateTimeKind.Utc);
    }
}
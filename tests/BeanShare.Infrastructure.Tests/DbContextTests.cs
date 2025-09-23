using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using BeanShare.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BeanShare.Infrastructure.Tests;

public sealed class DbContextTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public DbContextTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task ValueObjects_ShouldRoundTripCorrectly()
    {
        await using var context = await _databaseFixture.CreateDbContextAsync();
        var clock = new TestClock();
        
        var spaceId = SpaceId.New();
        var userId = new UserId(Guid.NewGuid());
        var inviteCode = new InviteCode("DEFGHJ");
        
        var space = Space.Create(spaceId, "Value Object Test", userId, inviteCode, clock);

        context.Spaces.Add(space);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var retrieved = await context.Spaces.FindAsync(spaceId);
        
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(spaceId);
        retrieved.InviteCode.Should().Be(inviteCode);
        retrieved.Members.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task SpaceMemberships_ShouldBeMappedAsOwnedEntities()
    {
        await using var context = await _databaseFixture.CreateDbContextAsync();
        var clock = new TestClock();
        
        var spaceId = SpaceId.New();
        var creatorId = new UserId(Guid.NewGuid());
        var memberId = new UserId(Guid.NewGuid());
        var inviteCode = new InviteCode("KLMNPQ");
        
        var space = Space.Create(spaceId, "Owned Entity Test", creatorId, inviteCode, clock);
        space.Join(memberId, clock);

        context.Spaces.Add(space);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var retrieved = await context.Spaces.FindAsync(spaceId);
        
        retrieved.Should().NotBeNull();
        retrieved!.Members.Should().HaveCount(2);
        retrieved.Members.Should().Contain(m => m.UserId == creatorId);
        retrieved.Members.Should().Contain(m => m.UserId == memberId);
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2025, 9, 14, 12, 0, 0, DateTimeKind.Utc);
    }
}
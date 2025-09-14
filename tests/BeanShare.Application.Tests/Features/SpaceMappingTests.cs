using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Application.Features.Spaces.Mapping;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using Mapster;
using NSubstitute;

namespace BeanShare.Application.Tests.Features;

public class SpaceMappingTests
{
    private readonly TypeAdapterConfig _config;

    public SpaceMappingTests()
    {
        _config = new TypeAdapterConfig();
        new SpaceMappingProfile().Register(_config);
    }

    [Fact]
    public void Space_ToSpaceDto_ShouldMapCorrectly()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var userId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        
        var space = Space.Create(spaceId, "My Coffee Space", userId, inviteCode, clock);

        var dto = space.Adapt<SpaceDto>(_config);

        dto.Id.Should().Be(space.Id);
        dto.Name.Should().Be("My Coffee Space");
        dto.InviteCode.Should().Be(space.InviteCode.Value);
        dto.CreatedBy.Should().Be(userId);
        dto.MemberCount.Should().Be(1);
        dto.Members.Should().HaveCount(1);
    }

    [Fact]
    public void Space_ToSpaceSummaryDto_ShouldMapCorrectly()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var userId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        
        var space = Space.Create(spaceId, "Coffee Team", userId, inviteCode, clock);

        var summary = space.Adapt<SpaceSummaryDto>(_config);

        summary.Id.Should().Be(space.Id);
        summary.Name.Should().Be("Coffee Team");
        summary.InviteCode.Should().Be(space.InviteCode.Value);
        summary.MemberCount.Should().Be(1);
        summary.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SpaceMembership_ToMembershipDto_ShouldMapCorrectly()
    {
        var userId = new UserId(Guid.NewGuid());
        var joinedAt = DateTime.UtcNow;
        
        // Create a space to get a membership
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(joinedAt);
        
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        var space = Space.Create(spaceId, "Test Space", userId, inviteCode, clock);
        var membership = space.Members.First();

        var dto = membership.Adapt<MembershipDto>(_config);

        dto.UserId.Should().Be(userId);
        dto.Email.Should().Be("user@example.com"); // Placeholder value
        dto.Role.Should().Be("Admin"); // Creator is admin
        dto.JoinedAt.Should().BeCloseTo(joinedAt, TimeSpan.FromSeconds(1));
    }
}
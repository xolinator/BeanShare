using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Application.Features.Spaces.Mapping;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using Mapster;
using NSubstitute;

namespace BeanShare.Application.Tests.Features;

public class SpaceMappingTests
{
    [Fact]
    public void SpaceDto_MapsBasicProperties()
    {
        var config = new TypeAdapterConfig();
        new SpaceMappingProfile().Register(config);
        
        var userId = new UserId(Guid.NewGuid());
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var space = Space.Create(SpaceId.New(), "Coffee Space", userId, new InviteCode("CAFE23"), clock);
        var dto = space.Adapt<SpaceDto>(config);

        dto.Name.Should().Be("Coffee Space");
        dto.MemberCount.Should().Be(1);
    }

    [Fact] 
    public void SpaceSummaryDto_MapsEssentials()
    {
        var config = new TypeAdapterConfig();
        new SpaceMappingProfile().Register(config);
        
        var userId = new UserId(Guid.NewGuid());
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var space = Space.Create(SpaceId.New(), "Team Space", userId, new InviteCode("CAFE23"), clock);
        var summary = space.Adapt<SpaceSummaryDto>(config);

        summary.Name.Should().Be("Team Space");
        summary.MemberCount.Should().Be(1);
    }
}
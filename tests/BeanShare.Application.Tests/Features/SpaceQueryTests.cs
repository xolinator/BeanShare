using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Application.Features.Spaces.Queries;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using NSubstitute;

namespace BeanShare.Application.Tests.Features;

public class SpaceQueryTests
{
    [Fact]
    public async Task GetSpaceById_WithValidId_ShouldReturnSpace()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var mapper = Substitute.For<IMapper>();
        var clock = Substitute.For<IClock>();
        
        var userId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        
        userContext.CurrentUserId.Returns(userId);
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var space = Space.Create(spaceId, "Test Space", userId, inviteCode, clock);
        spaceRepository.GetByIdAsync(spaceId, default).Returns(space);
        
        var spaceDto = new SpaceDto 
        { 
            Id = spaceId,
            Name = "Test Space", 
            InviteCode = "CAFE23",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            MemberCount = 1,
            Members = new List<MembershipDto>()
        };
        mapper.Map<SpaceDto>(space).Returns(spaceDto);

        var handler = new GetSpaceByIdQueryHandler(spaceRepository, userContext, mapper);
        var query = new GetSpaceByIdQuery(spaceId);

        var result = await handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Space");
        result.Value.InviteCode.Should().Be("CAFE23");
    }

    [Fact]
    public async Task GetSpaceById_WithNonExistentId_ShouldReturnError()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var mapper = Substitute.For<IMapper>();
        
        var spaceId = SpaceId.New();
        spaceRepository.GetByIdAsync(spaceId, default).Returns((Space?)null);

        var handler = new GetSpaceByIdQueryHandler(spaceRepository, userContext, mapper);
        var query = new GetSpaceByIdQuery(spaceId);

        var result = await handler.Handle(query, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("SPACE_NOT_FOUND");
    }

    [Fact]
    public async Task GetUserSpaces_WithValidUser_ShouldReturnSpaces()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var mapper = Substitute.For<IMapper>();
        var clock = Substitute.For<IClock>();
        
        var userId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        
        userContext.CurrentUserId.Returns(userId);
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var space = Space.Create(spaceId, "Test Space", userId, inviteCode, clock);
        var spaces = new List<Space> { space };
        
        spaceRepository.GetUserSpacesAsync(userId, default).Returns(spaces);
        
        var spaceSummary = new SpaceSummaryDto 
        { 
            Id = spaceId, 
            Name = "Test Space",
            InviteCode = "CAFE23",
            MemberCount = 1,
            CreatedAt = DateTime.UtcNow
        };
        mapper.Map<IReadOnlyList<SpaceSummaryDto>>(spaces).Returns(new List<SpaceSummaryDto> { spaceSummary });

        var handler = new GetUserSpacesQueryHandler(spaceRepository, userContext, mapper);
        var query = new GetUserSpacesQuery();

        var result = await handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Spaces.Should().HaveCount(1);
        result.Value.Spaces.First().Name.Should().Be("Test Space");
    }
}
using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Spaces.Commands;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BeanShare.Application.Tests.Features;

public class SpaceCommandTests
{
    [Fact]
    public async Task CreateSpace_WithValidName_ShouldSucceed()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var inviteCodeGenerator = Substitute.For<IInviteCodeGenerator>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();
        
        var userId = new UserId(Guid.NewGuid());
        var inviteCode = new InviteCode("CAFE23");
        
        userContext.CurrentUserId.Returns(userId);
        inviteCodeGenerator.GenerateAsync(default).Returns(inviteCode);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var handler = new CreateSpaceCommandHandler(spaceRepository, inviteCodeGenerator, userContext, clock);
        var command = new CreateSpaceCommand("My Coffee Space");

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.InviteCode.Should().Be("CAFE23");
        await spaceRepository.Received(1).AddAsync(Arg.Any<Space>(), default);
    }

    [Fact]
    public async Task JoinSpace_WithValidCode_ShouldSucceed()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();
        
        var userId = new UserId(Guid.NewGuid());
        var creatorId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");
        
        userContext.CurrentUserId.Returns(userId);
        clock.UtcNow.Returns(DateTime.UtcNow);
        
        var space = Space.Create(spaceId, "Test Space", creatorId, inviteCode, clock);
        spaceRepository.GetByInviteCodeAsync(inviteCode, default).Returns(space);

        var handler = new JoinSpaceCommandHandler(spaceRepository, userContext, clock);
        var command = new JoinSpaceCommand("CAFE23");

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.SpaceName.Should().Be("Test Space");
    }
}
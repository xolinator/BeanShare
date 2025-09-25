using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Spaces.Commands;
using BeanShare.Application.Tests.TestHelpers;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
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
        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new JoinSpaceCommandHandler(spaceRepository, userContext, clock);
        var command = new JoinSpaceCommand("CAFE23");

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.SpaceName.Should().Be("Test Space");
    }

    [Fact]
    public async Task PromoteMember_WithValidRequest_ShouldPromoteMember()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var memberId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);
        space.Join(memberId, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new PromoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new PromoteMemberCommand(spaceId.Value, memberId.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Admin");
        space.IsAdmin(memberId).Should().BeTrue();
        await spaceRepository.Received(1).UpdateAsync(space, default);
    }

    [Fact]
    public async Task PromoteMember_WithNonAdminCaller_ShouldReturnUnauthorized()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var memberId = new UserId(Guid.NewGuid());
        var unauthorizedUserId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(unauthorizedUserId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);
        space.Join(memberId, clock);
        space.Join(unauthorizedUserId, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new PromoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new PromoteMemberCommand(spaceId.Value, memberId.Value);

        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "INSUFFICIENT_PRIVILEGES");
        await spaceRepository.DidNotReceive().UpdateAsync(Arg.Any<Space>(), default);
    }

    [Fact]
    public async Task PromoteMember_WithNonMember_ShouldReturnMemberNotFound()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var nonMemberId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new PromoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new PromoteMemberCommand(spaceId.Value, nonMemberId.Value);

        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "MEMBER_NOT_FOUND");
        await spaceRepository.DidNotReceive().UpdateAsync(Arg.Any<Space>(), default);
    }

    [Fact]
    public async Task PromoteMember_WithExistingAdmin_ShouldBeIdempotent()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var targetAdminId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);
        space.Join(targetAdminId, clock);
        space.PromoteMember(targetAdminId, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new PromoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new PromoteMemberCommand(spaceId.Value, targetAdminId.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Admin");
        space.IsAdmin(targetAdminId).Should().BeTrue();
    }

    [Fact]
    public async Task DemoteMember_WithValidRequest_ShouldDemoteMember()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var targetAdminId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);
        space.Join(targetAdminId, clock);
        space.PromoteMember(targetAdminId, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new DemoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new DemoteMemberCommand(spaceId.Value, targetAdminId.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Member");
        space.IsAdmin(targetAdminId).Should().BeFalse();
        await spaceRepository.Received(1).UpdateAsync(space, default);
    }

    [Fact]
    public async Task DemoteMember_WithLastAdmin_ShouldReturnLastAdminViolation()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new DemoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new DemoteMemberCommand(spaceId.Value, adminId.Value);

        var result = await handler.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "LAST_ADMIN_PROTECTION");
        await spaceRepository.DidNotReceive().UpdateAsync(Arg.Any<Space>(), default);
    }

    [Fact]
    public async Task DemoteMember_WithExistingMember_ShouldBeIdempotent()
    {
        var spaceRepository = Substitute.For<ISpaceRepository>();
        var userContext = Substitute.For<IUserContext>();
        var clock = Substitute.For<IClock>();

        var adminId = new UserId(Guid.NewGuid());
        var memberId = new UserId(Guid.NewGuid());
        var spaceId = SpaceId.New();
        var inviteCode = new InviteCode("CAFE23");

        userContext.CurrentUserId.Returns(adminId);
        clock.UtcNow.Returns(DateTime.UtcNow);

        var space = Space.Create(spaceId, "Test Space", adminId, inviteCode, clock);
        space.Join(memberId, clock);

        spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(space);

        var handler = new DemoteMemberCommandHandler(spaceRepository, userContext, clock);
        var command = new DemoteMemberCommand(spaceId.Value, memberId.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Member");
        space.IsAdmin(memberId).Should().BeFalse();
    }
}
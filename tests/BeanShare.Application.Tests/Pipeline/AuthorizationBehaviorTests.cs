using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Application.Pipeline;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace BeanShare.Application.Tests.Pipeline;

public sealed class AuthorizationBehaviorTests
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly ISpaceRepository _spaceRepository = Substitute.For<ISpaceRepository>();
    private readonly AuthorizationBehavior<TestRequest, Result<string>> _behavior;
    private readonly RequestHandlerDelegate<Result<string>> _next = Substitute.For<RequestHandlerDelegate<Result<string>>>();

    public AuthorizationBehaviorTests()
    {
        _behavior = new AuthorizationBehavior<TestRequest, Result<string>>(_userContext, _spaceRepository);
        _next.Invoke().Returns(Result<string>.Success("success"));
    }

    [Fact]
    public async Task Handle_RequestWithoutIAuthorize_ShouldPassThrough()
    {
        var request = new NonAuthRequest();
        var behavior = new AuthorizationBehavior<NonAuthRequest, Result<string>>(_userContext, _spaceRepository);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Result<string>.Success("success"));

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldReturnUnauthenticated()
    {
        var request = new TestRequestWithoutAttributes();
        var behavior = new AuthorizationBehavior<TestRequestWithoutAttributes, Result<string>>(_userContext, _spaceRepository);

        _userContext.CurrentUserId.Returns(_ => throw new InvalidOperationException("User is not authenticated"));

        var result = await behavior.Handle(request, Substitute.For<RequestHandlerDelegate<Result<string>>>(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "auth.unauthenticated");
    }

    [Fact]
    public async Task Handle_RequireSpaceMember_UserIsMember_ShouldSucceed()
    {
        var spaceId = SpaceId.New();
        var userId = UserId.New();
        var request = new TestRequest { SpaceId = spaceId.Value };

        var space = Space.Create(SpaceId.New(), "Test Space", userId, new InviteCode("CAFE23"), Substitute.For<IClock>());

        _userContext.CurrentUserId.Returns(userId);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(space);

        var result = await _behavior.Handle(request, _next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_RequireSpaceAdmin_UserIsAdmin_ShouldSucceed()
    {
        var spaceId = SpaceId.New();
        var userId = UserId.New();
        var request = new AdminTestRequest { SpaceId = spaceId.Value };

        var space = Space.Create(SpaceId.New(), "Test Space", userId, new InviteCode("CAFE23"), Substitute.For<IClock>());
        var behavior = new AuthorizationBehavior<AdminTestRequest, Result<string>>(_userContext, _spaceRepository);

        _userContext.CurrentUserId.Returns(userId);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(space);

        var result = await behavior.Handle(request, _next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_RequireSpaceAdmin_UserNotAdmin_ShouldReturnNotAdmin()
    {
        var spaceId = SpaceId.New();
        var adminUserId = UserId.New();
        var memberUserId = UserId.New();
        var request = new AdminTestRequest { SpaceId = spaceId.Value };

        var space = Space.Create(SpaceId.New(), "Test Space", adminUserId, new InviteCode("CAFE23"), Substitute.For<IClock>());
        space.Join(memberUserId, Substitute.For<IClock>());

        var behavior = new AuthorizationBehavior<AdminTestRequest, Result<string>>(_userContext, _spaceRepository);

        _userContext.CurrentUserId.Returns(memberUserId);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(space);

        var result = await behavior.Handle(request, _next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "auth.not_admin");
        await _next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_SpaceNotFound_ShouldReturnSpaceNotFound()
    {
        var spaceId = SpaceId.New();
        var userId = UserId.New();
        var request = new TestRequest { SpaceId = spaceId.Value };

        _userContext.CurrentUserId.Returns(userId);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>()).Returns((Space?)null);

        var result = await _behavior.Handle(request, _next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "auth.space_not_found");
        await _next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_InvalidSpaceIdProperty_ShouldReturnInvalidRequest()
    {
        var userId = UserId.New();
        var request = new InvalidPropertyTestRequest();

        _userContext.CurrentUserId.Returns(userId);

        var behavior = new AuthorizationBehavior<InvalidPropertyTestRequest, Result<string>>(_userContext, _spaceRepository);

        var result = await behavior.Handle(request, _next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "auth.invalid_request");
        await _next.DidNotReceive().Invoke();
    }
}

public record NonAuthRequest() : IRequest<Result<string>>;

public record TestRequestWithoutAttributes() : IAuthorize, IRequest<Result<string>>;

[RequireSpaceMember("SpaceId")]
public record TestRequest : IAuthorize, IRequest<Result<string>>
{
    public Guid SpaceId { get; init; }
}

[RequireSpaceAdmin("SpaceId")]
public record AdminTestRequest : IAuthorize, IRequest<Result<string>>
{
    public Guid SpaceId { get; init; }
}

[RequireSpaceMember("NonExistentProperty")]
public record InvalidPropertyTestRequest : IAuthorize, IRequest<Result<string>>;

[RequireSpaceMember("SpaceId")]
public record BaseTestRequest : IAuthorize, IRequest<Result<string>>
{
    public Guid SpaceId { get; init; }
}

public class TestHandler : IRequestHandler<TestRequest, Result<string>>
{
    public Task<Result<string>> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<string>.Success("test result"));
    }
}
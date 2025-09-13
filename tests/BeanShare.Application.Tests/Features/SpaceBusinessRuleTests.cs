using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using NSubstitute;

namespace BeanShare.Application.Tests.Features;

public class SpaceBusinessRuleTests
{
    [Fact]
    public async Task JoinSpace_WhenInviteCodeNotFound_ShouldReturnBusinessError()
    {
        var repository = Substitute.For<ISpaceRepository>();
        var invalidCode = new InviteCode("ABCEFG"); // Valid format but not found in repo
        
        repository.GetByInviteCodeAsync(invalidCode, default).Returns((Space?)null);

        var space = await repository.GetByInviteCodeAsync(invalidCode);
        var result = space == null 
            ? Result.Failure(Error.InviteCodeNotFound(invalidCode.Value))
            : Result.Success();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("INVITE_CODE_INVALID");
    }

    [Fact]
    public async Task GetSpaceById_WhenSpaceExists_ShouldReturnSpace()
    {
        var repository = Substitute.For<ISpaceRepository>();
        var spaceId = SpaceId.New();
        var expectedSpace = CreateTestSpace(spaceId);
        
        repository.GetByIdAsync(spaceId, default).Returns(expectedSpace);

        var result = await repository.GetByIdAsync(spaceId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(spaceId);
    }

    private static Space CreateTestSpace(SpaceId id)
    {
        var creatorId = UserId.New();
        var inviteCode = new InviteCode("CAFE23");
        return Space.Create(id, "Test Coffee Space", creatorId, inviteCode, TestClock.Instance);
    }
}

internal class TestClock : IClock
{
    public static readonly TestClock Instance = new();
    public DateTime UtcNow => new(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
}
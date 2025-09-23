using BeanShare.Application.Common;

namespace BeanShare.Application.Tests;

public class ResultIntegrationTests
{
    [Fact]
    public void SpaceBusinessRules_WhenViolated_ShouldReturnSpecificErrors()
    {
        var spaceNotFoundError = Error.SpaceNotFound(Guid.NewGuid());
        var alreadyMemberError = Error.AlreadySpaceMember(Guid.NewGuid(), Guid.NewGuid());

        var result = Result.Combine(
            Result.Failure(spaceNotFoundError),
            Result.Failure(alreadyMemberError)
        );

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == Error.Codes.SpaceNotFound);
        result.Errors.Should().Contain(e => e.Code == Error.Codes.AlreadyMember);
    }

    [Fact]
    public void ResultChaining_ForBusinessOperations_ShouldHandleFailureGracefully()
    {
        var initialResult = Result<string>.Success("ValidSpaceName");

        var chainedResult = initialResult
            .Bind(name => name.Length > 0 
                ? Result<string>.Success(name.ToUpper()) 
                : Result<string>.Failure(Error.InvalidSpaceName(name)))
            .Map(name => new { SpaceName = name, CreatedAt = DateTime.UtcNow });

        chainedResult.IsSuccess.Should().BeTrue();
        chainedResult.Value.SpaceName.Should().Be("VALIDSPACENAME");
    }
}
using BeanShare.Application.Abstractions;
using BeanShare.Application.Behaviors;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using FluentValidation;
using NSubstitute;

namespace BeanShare.Application.Tests.Behaviors;

public class PipelineBehaviorTests
{
    [Fact]
    public async Task AuthorizationBehavior_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.CurrentUserId.Returns(new UserId(Guid.Empty));

        var behavior = new AuthorizationBehavior<TestCreateCommand, string>(userContext);
        var result = await behavior.Handle(new TestCreateCommand(), () => Task.FromResult(Result<string>.Success("should-not-reach")), default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be(Error.Codes.Unauthorized);
    }

    [Fact]
    public async Task ValidationBehavior_WithValidationFailure_ShouldReturnValidationError()
    {
        var validator = Substitute.For<IValidator<TestCreateCommand>>();
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Name", "Coffee space name is required"));
        
        validator.ValidateAsync(Arg.Any<ValidationContext<TestCreateCommand>>(), Arg.Any<CancellationToken>())
                .Returns(validationResult);

        var behavior = new ValidationBehavior<TestCreateCommand, string>(new[] { validator });
        var result = await behavior.Handle(new TestCreateCommand(), () => Task.FromResult(Result<string>.Success("should-not-reach")), default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be(Error.Codes.ValidationError);
    }

    [Fact]
    public async Task UnitOfWorkBehavior_SavesOnlyOnSuccessfulCommand()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestCreateCommand, string>(unitOfWork);
        
        var successResult = Result<string>.Success("created");
        await behavior.Handle(new TestCreateCommand(), () => Task.FromResult(successResult), default);
        await unitOfWork.Received(1).SaveChangesAsync(default);

        unitOfWork.ClearReceivedCalls();
        var failureResult = Result<string>.Failure(Error.ValidationFailure("Name", "Required"));
        await behavior.Handle(new TestCreateCommand(), () => Task.FromResult(failureResult), default);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public record TestCreateCommand : ICommand<Result<string>>;
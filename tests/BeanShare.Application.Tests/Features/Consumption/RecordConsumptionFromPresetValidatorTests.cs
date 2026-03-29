using BeanShare.Application.Features.Consumption.Commands;
using BeanShare.Application.Tests.Features;
using BeanShare.Domain.Common;
using FluentValidation.TestHelper;
using Xunit;

namespace BeanShare.Application.Tests.Features.Consumption;

public class RecordConsumptionFromPresetValidatorTests
{
    private readonly IClock _clock = TestClock.Instance;
    private readonly RecordConsumptionFromPresetValidator _validator;

    public RecordConsumptionFromPresetValidatorTests()
    {
        _validator = new RecordConsumptionFromPresetValidator(_clock);
    }

    [Fact]
    public void Should_Have_Error_When_SpaceId_Is_Empty()
    {
        var command = new RecordConsumptionFromPresetCommand(
            Guid.Empty,
            Guid.NewGuid(),
            null,
            _clock.UtcNow);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SpaceId);
    }

    [Fact]
    public void Should_Have_Error_When_PresetId_Is_Empty()
    {
        var command = new RecordConsumptionFromPresetCommand(
            Guid.NewGuid(),
            Guid.Empty,
            null,
            _clock.UtcNow);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PresetId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Have_Error_When_CustomQuantity_Is_Out_Of_Range(decimal quantity)
    {
        var command = new RecordConsumptionFromPresetCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity,
            _clock.UtcNow);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomQuantityGrams);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new RecordConsumptionFromPresetCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            20,
            _clock.UtcNow);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
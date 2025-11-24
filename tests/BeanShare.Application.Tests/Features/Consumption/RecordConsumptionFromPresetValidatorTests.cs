using BeanShare.Application.Features.Consumption.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace BeanShare.Application.Tests.Features.Consumption;

public class RecordConsumptionFromPresetValidatorTests
{
    private readonly RecordConsumptionFromPresetValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_SpaceId_Is_Empty()
    {
        var command = new RecordConsumptionFromPresetCommand(
            Guid.Empty,
            Guid.NewGuid(),
            null,
            DateTime.UtcNow);

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
            DateTime.UtcNow);

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
            DateTime.UtcNow);

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
            DateTime.UtcNow);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
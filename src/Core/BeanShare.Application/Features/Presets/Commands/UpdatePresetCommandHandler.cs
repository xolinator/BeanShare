using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;

public sealed class UpdatePresetCommandHandler : IRequestHandler<UpdatePresetCommand, Result>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly IUserContext _userContext;

    public UpdatePresetCommandHandler(
        IPresetRecipeRepository presetRepository,
        IUserContext userContext)
    {
        _presetRepository = presetRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdatePresetCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(Error.ValidationFailure(nameof(request.Name), "Preset name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.CoffeeType))
        {
            return Result.Failure(Error.ValidationFailure(nameof(request.CoffeeType), "Coffee type is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Brand))
        {
            return Result.Failure(Error.ValidationFailure(nameof(request.Brand), "Brand is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Preparation))
        {
            return Result.Failure(Error.ValidationFailure(nameof(request.Preparation), "Preparation method is required"));
        }

        if (request.DefaultGrams <= 0)
        {
            return Result.Failure(Error.ValidationFailure(nameof(request.DefaultGrams), "Default grams must be positive"));
        }

        var presetId = new PresetRecipeId(request.PresetId);
        var preset = await _presetRepository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("PresetRecipe", "Preset not found"));
        }

        if (preset.UserId != _userContext.CurrentUserId)
        {
            return Result.Failure(Error.Forbidden("PresetRecipe", "You cannot update presets created by other users"));
        }

        try
        {
            var defaultGrams = Weight.FromGrams(request.DefaultGrams);

            preset.Update(
                request.Name.Trim(),
                request.CoffeeType.Trim(),
                request.Brand.Trim(),
                request.Preparation.Trim(),
                defaultGrams,
                request.Notes?.Trim(),
                request.IsShared);

            await _presetRepository.UpdateAsync(preset, cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.ValidationFailure("PresetRecipe", ex.Message));
        }
    }
}
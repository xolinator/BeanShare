using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;

public sealed class DeletePresetCommandHandler : IRequestHandler<DeletePresetCommand, Result>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly IUserContext _userContext;

    public DeletePresetCommandHandler(
        IPresetRecipeRepository presetRepository,
        IUserContext userContext)
    {
        _presetRepository = presetRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(DeletePresetCommand request, CancellationToken cancellationToken)
    {
        var presetId = new PresetRecipeId(request.PresetId);
        var preset = await _presetRepository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("PresetRecipe", "Preset not found"));
        }

        if (preset.UserId != _userContext.CurrentUserId)
        {
            return Result.Failure(Error.Forbidden("PresetRecipe", "You cannot delete presets created by other users"));
        }

        await _presetRepository.DeleteAsync(presetId, cancellationToken);

        return Result.Success();
    }
}
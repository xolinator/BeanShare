using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed class ToggleGlobalPresetCommandHandler : IRequestHandler<ToggleGlobalPresetCommand, Result>
{
    private readonly ISpaceGlobalPresetConfigRepository _configRepository;
    private readonly IGlobalPresetRepository _globalPresetRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public ToggleGlobalPresetCommandHandler(
        ISpaceGlobalPresetConfigRepository configRepository,
        IGlobalPresetRepository globalPresetRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _configRepository = configRepository;
        _globalPresetRepository = globalPresetRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result> Handle(ToggleGlobalPresetCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var globalPresetId = new GlobalPresetId(request.GlobalPresetId);
        var userId = _userContext.CurrentUserId;

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
        {
            return Result.Failure(Error.NotFound("Space", "Space not found"));
        }

        if (!space.IsAdmin(userId))
        {
            return Result.Failure(Error.Forbidden("Space", "Only space admins can toggle global presets"));
        }

        var presetExists = await _globalPresetRepository.ExistsAsync(globalPresetId, cancellationToken);
        if (!presetExists)
        {
            return Result.Failure(Error.NotFound("GlobalPreset", "Global preset not found"));
        }

        var existingConfig = await _configRepository.GetBySpaceAndPresetAsync(spaceId, globalPresetId, cancellationToken);

        if (existingConfig is null)
        {
            var config = SpaceGlobalPresetConfig.Create(
                spaceId,
                globalPresetId,
                request.IsEnabled,
                _clock.UtcNow);
            await _configRepository.AddAsync(config, cancellationToken);
        }
        else
        {
            existingConfig.SetEnabled(request.IsEnabled, _clock.UtcNow);
            await _configRepository.UpdateAsync(existingConfig, cancellationToken);
        }

        return Result.Success();
    }
}

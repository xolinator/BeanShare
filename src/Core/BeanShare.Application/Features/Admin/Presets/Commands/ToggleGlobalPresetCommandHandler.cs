using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Entities;
using MediatR;

namespace BeanShare.Application.Features.Admin.Presets.Commands;
public sealed class ToggleGlobalPresetCommandHandler : IRequestHandler<ToggleGlobalPresetCommand, Result>
{
    private readonly IGlobalPresetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleGlobalPresetCommandHandler(IGlobalPresetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ToggleGlobalPresetCommand request, CancellationToken cancellationToken)
    {
        var presetId = new GlobalPresetId(request.Id);
        var preset = await _repository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("Preset.NotFound", "Preset not found"));
        }

        if (request.Activate)
        {
            preset.Activate();
        }
        else
        {
            preset.Deactivate();
        }

        await _repository.UpdateAsync(preset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

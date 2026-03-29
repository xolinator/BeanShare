using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Admin.Presets.Commands;
public sealed class UpdateGlobalPresetCommandHandler : IRequestHandler<UpdateGlobalPresetCommand, Result>
{
    private readonly IGlobalPresetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGlobalPresetCommandHandler(IGlobalPresetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateGlobalPresetCommand request, CancellationToken cancellationToken)
    {
        var presetId = new GlobalPresetId(request.Id);
        var preset = await _repository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("Preset.NotFound", "Preset not found"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(Error.ValidationFailure("Name", "Preset name is required"));
        }

        if (request.DefaultGrams <= 0)
        {
            return Result.Failure(Error.ValidationFailure("DefaultGrams", "Default grams must be positive"));
        }

        preset.Update(
            request.Name,
            request.DefaultCoffeeType,
            request.DefaultPreparation,
            Weight.FromGrams(request.DefaultGrams),
            request.Description,
            request.DisplayOrder);

        await _repository.UpdateAsync(preset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

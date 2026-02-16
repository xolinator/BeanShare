using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Admin.Presets.Commands;
public sealed class CreateGlobalPresetCommandHandler : IRequestHandler<CreateGlobalPresetCommand, Result<Guid>>
{
    private readonly IGlobalPresetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateGlobalPresetCommandHandler(
        IGlobalPresetRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<Guid>> Handle(CreateGlobalPresetCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Guid>.Failure(Error.ValidationFailure("Name", "Preset name is required"));
        }

        if (request.DefaultGrams <= 0)
        {
            return Result<Guid>.Failure(Error.ValidationFailure("DefaultGrams", "Default grams must be positive"));
        }

        var preset = GlobalPreset.Create(
            request.Name,
            request.DefaultCoffeeType,
            request.DefaultPreparation,
            Weight.FromGrams(request.DefaultGrams),
            request.DisplayOrder,
            _clock.UtcNow,
            request.Description);

        await _repository.AddAsync(preset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(preset.Id.Value);
    }
}

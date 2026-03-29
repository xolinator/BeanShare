using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Presets.Dtos;
using BeanShare.Application.Abstractions;
using MediatR;

namespace BeanShare.Application.Features.Admin.Presets.Queries;
public sealed class GetAllGlobalPresetsQueryHandler : IRequestHandler<GetAllGlobalPresetsQuery, Result<IReadOnlyList<GlobalPresetDto>>>
{
    private readonly IGlobalPresetRepository _repository;

    public GetAllGlobalPresetsQueryHandler(IGlobalPresetRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<GlobalPresetDto>>> Handle(GetAllGlobalPresetsQuery request, CancellationToken cancellationToken)
    {
        var presets = request.IncludeInactive
            ? await _repository.GetAllAsync(cancellationToken)
            : await _repository.GetAllActiveAsync(cancellationToken);

        var dtos = presets.Select(p => new GlobalPresetDto(
            p.Id.Value,
            p.Name,
            p.DefaultCoffeeType,
            p.DefaultPreparation,
            p.DefaultGrams.Grams,
            p.Description,
            p.DisplayOrder,
            p.IsActive,
            p.CreatedAt)).ToList();

        return Result<IReadOnlyList<GlobalPresetDto>>.Success(dtos);
    }
}

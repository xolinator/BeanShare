using BeanShare.Application.Features.Presets.Queries;
using BeanShare.Contracts.Presets;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Presets;

public sealed class GetQuickPresetsEndpoint : EndpointWithoutRequest<GetQuickPresetsResponse>
{
    private readonly IMediator _mediator;

    public GetQuickPresetsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{spaceId}/quick-presets");
        Summary(s =>
        {
            s.Summary = "Get quick presets for consumption form";
            s.Description = "Returns global and space presets available for quick consumption buttons";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var spaceId = Route<Guid>("spaceId");

        var query = new GetQuickPresetsQuery(spaceId);
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var response = new GetQuickPresetsResponse
        {
            Presets = result.Value.Presets.Select(p => new Contracts.Presets.QuickPresetDto
            {
                GlobalPresetId = p.GlobalPresetId,
                SpacePresetId = p.SpacePresetId,
                Name = p.Name,
                CoffeeType = p.CoffeeType,
                Preparation = p.Preparation,
                DefaultGrams = p.DefaultGrams,
                Description = p.Description,
                IsFavorite = p.IsFavorite,
                IsGlobal = p.IsGlobal,
                DisplayOrder = p.DisplayOrder
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}

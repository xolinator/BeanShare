using BeanShare.Application.Features.Presets.Queries;
using BeanShare.Contracts.Presets;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Presets;

public sealed class GetSpaceGlobalPresetsEndpoint : EndpointWithoutRequest<GetSpaceGlobalPresetsResponse>
{
    private readonly IMediator _mediator;

    public GetSpaceGlobalPresetsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{spaceId}/global-presets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all global presets with enabled/disabled status for a space";
            s.Description = "Returns all global presets with their enabled status for managing which are available in a space";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var spaceId = Route<Guid>("spaceId");
        var query = new GetSpaceGlobalPresetsQuery(spaceId);
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

        var response = new GetSpaceGlobalPresetsResponse
        {
            Presets = result.Value.Presets.Select(p => new BeanShare.Contracts.Presets.SpaceGlobalPresetDto
            {
                GlobalPresetId = p.GlobalPresetId,
                Name = p.Name,
                CoffeeType = p.CoffeeType,
                Preparation = p.Preparation,
                DefaultGrams = p.DefaultGrams,
                Description = p.Description,
                IsEnabled = p.IsEnabled,
                DisplayOrder = p.DisplayOrder
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}

using BeanShare.Application.Features.Presets.Queries;
using BeanShare.Contracts.Presets;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Presets;

public sealed class GetSpacePresetsEndpoint : EndpointWithoutRequest<GetSpacePresetsResponse>
{
    private readonly IMediator _mediator;

    public GetSpacePresetsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{spaceId}/presets");
        Summary(s =>
        {
            s.Summary = "Get all presets for a space";
            s.Description = "Returns all preset recipes available in a space (both personal and shared)";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var spaceId = Route<Guid>("spaceId");

        var query = new GetSpacePresetsQuery(spaceId);
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

        var response = new GetSpacePresetsResponse
        {
            Presets = result.Value.Presets.Select(p => new Contracts.Presets.PresetDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.UserName,
                Name = p.Name,
                CoffeeType = p.CoffeeType,
                Preparation = p.Preparation,
                DefaultGrams = p.DefaultGrams,
                Notes = p.Notes,
                IsShared = p.IsShared,
                IsOwner = p.IsOwner,
                CreatedAt = p.CreatedAt,
                LastUsedAt = p.LastUsedAt,
                UsageCount = p.UsageCount
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}
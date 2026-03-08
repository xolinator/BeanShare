using BeanShare.Application.Features.Admin.Presets.Queries;
using BeanShare.Contracts.Admin;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Admin.Presets;

public sealed class GetAllGlobalPresetsEndpoint : EndpointWithoutRequest<GetAllGlobalPresetsResponse>
{
    private readonly IMediator _mediator;

    public GetAllGlobalPresetsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/admin/presets");
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "Get all global presets (Admin)";
            s.Description = "Retrieve all global presets including inactive ones";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetAllGlobalPresetsQuery(IncludeInactive: true);
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

        var response = new GetAllGlobalPresetsResponse
        {
            Presets = result.Value.Select(p => new GlobalPresetItem
            {
                Id = p.Id,
                Name = p.Name,
                DefaultCoffeeType = p.DefaultCoffeeType,
                DefaultPreparation = p.DefaultPreparation,
                DefaultGrams = p.DefaultGrams,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}

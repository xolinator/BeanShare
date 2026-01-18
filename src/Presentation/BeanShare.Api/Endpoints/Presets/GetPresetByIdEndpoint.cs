using BeanShare.Application.Features.Presets.Queries;
using BeanShare.Contracts.Presets;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Presets;

public sealed class GetPresetByIdEndpoint : EndpointWithoutRequest<Contracts.Presets.PresetDto>
{
    private readonly IMediator _mediator;

    public GetPresetByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/presets/{id}");
        Summary(s =>
        {
            s.Summary = "Get a preset by ID";
            s.Description = "Returns a specific preset recipe by its ID";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var presetId = Route<Guid>("id");

        var query = new GetPresetByIdQuery(presetId);
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

        var preset = result.Value;
        var response = new Contracts.Presets.PresetDto
        {
            Id = preset.Id,
            UserId = preset.UserId,
            UserName = preset.UserName,
            Name = preset.Name,
            CoffeeType = preset.CoffeeType,
            Preparation = preset.Preparation,
            DefaultGrams = preset.DefaultGrams,
            Notes = preset.Notes,
            IsShared = preset.IsShared,
            IsOwner = preset.IsOwner,
            CreatedAt = preset.CreatedAt,
            LastUsedAt = preset.LastUsedAt,
            UsageCount = preset.UsageCount
        };

        await SendOkAsync(response, ct);
    }
}
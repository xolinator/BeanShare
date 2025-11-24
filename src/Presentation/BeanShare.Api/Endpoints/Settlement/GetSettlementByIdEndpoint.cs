using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Features.Settlement.Queries.GetSettlementById;
using BeanShare.Domain.ValueObjects;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Settlement;

public sealed class GetSettlementByIdRequest
{
    public Guid Id { get; init; }
}

public sealed class GetSettlementByIdEndpoint : Endpoint<GetSettlementByIdRequest, SettlementDto>
{
    private readonly IMediator _mediator;

    public GetSettlementByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/settlements/{Id}");
        Summary(s =>
        {
            s.Summary = "Get settlement by ID";
            s.Description = "Returns detailed information about a specific settlement";
        });
    }

    public override async Task HandleAsync(GetSettlementByIdRequest req, CancellationToken ct)
    {
        var query = new GetSettlementByIdQuery(new SettlementId(req.Id));
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendResultAsync(Results.BadRequest(result.Errors.Any() ? result.Errors.First().Message : "Failed to get settlement"));
        }
    }
}
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Features.Settlement.Queries.GetSpaceSettlements;
using BeanShare.Domain.ValueObjects;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Settlement;

public sealed class GetSpaceSettlementsRequest
{
    public Guid SpaceId { get; init; }
}

public sealed class GetSpaceSettlementsEndpoint : Endpoint<GetSpaceSettlementsRequest, List<SettlementSummaryDto>>
{
    private readonly IMediator _mediator;

    public GetSpaceSettlementsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{SpaceId}/settlements");
        AllowAnonymous(); // TODO: Replace with proper OIDC authentication
        Summary(s =>
        {
            s.Summary = "Get all settlements for a space";
            s.Description = "Returns a list of settlement summaries for the specified space";
        });
    }

    public override async Task HandleAsync(GetSpaceSettlementsRequest req, CancellationToken ct)
    {
        var query = new GetSpaceSettlementsQuery(new SpaceId(req.SpaceId));
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendResultAsync(Results.BadRequest(result.Errors.Any() ? result.Errors.First().Message : "Failed to get settlements"));
        }
    }
}
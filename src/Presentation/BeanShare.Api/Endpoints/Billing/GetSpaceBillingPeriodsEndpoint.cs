using BeanShare.Application.Features.Billing.Dtos;
using BeanShare.Application.Features.Billing.Queries.GetSpaceBillingPeriods;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Billing;

public sealed class GetSpaceBillingPeriodsRequest
{
    public Guid SpaceId { get; set; }
}

public sealed class GetSpaceBillingPeriodsEndpoint : Endpoint<GetSpaceBillingPeriodsRequest, List<BillingPeriodSummaryDto>>
{
    private readonly IMediator _mediator;

    public GetSpaceBillingPeriodsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{SpaceId}/billing-periods");
        AllowAnonymous(); // TODO: Require authentication
    }

    public override async Task HandleAsync(GetSpaceBillingPeriodsRequest req, CancellationToken ct)
    {
        var query = new GetSpaceBillingPeriodsQuery(req.SpaceId);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendResultAsync(TypedResults.Problem(
                detail: result.Errors.Any() ? result.Errors.First().Message : "Failed to retrieve billing periods",
                statusCode: 400
            ));
        }
    }
}
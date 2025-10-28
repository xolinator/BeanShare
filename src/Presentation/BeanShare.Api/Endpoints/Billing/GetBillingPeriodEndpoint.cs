using BeanShare.Application.Features.Billing.Dtos;
using BeanShare.Application.Features.Billing.Queries.GetBillingPeriodById;
using BeanShare.Domain.ValueObjects;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Billing;

public sealed class GetBillingPeriodRequest
{
    public Guid BillingPeriodId { get; set; }
}

public sealed class GetBillingPeriodEndpoint : Endpoint<GetBillingPeriodRequest, BillingPeriodDto>
{
    private readonly IMediator _mediator;

    public GetBillingPeriodEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/billing-periods/{BillingPeriodId}");
        AllowAnonymous(); // TODO: Require authentication
        Summary(s =>
        {
            s.Summary = "Get billing period details";
            s.Description = "Retrieves detailed information about a specific billing period";
            s.Response<BillingPeriodDto>(200, "Billing period found");
            s.Response(404, "Billing period not found");
        });
    }

    public override async Task HandleAsync(GetBillingPeriodRequest req, CancellationToken ct)
    {
        var query = new GetBillingPeriodByIdQuery(new BillingPeriodId(req.BillingPeriodId));
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(result.Value, ct);
    }
}
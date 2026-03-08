using BeanShare.Application.Features.Consumption.Queries.GetUserConsumptionHistory;
using BeanShare.Application.Features.Consumption.Dtos;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Consumption;

public sealed class GetUserConsumptionHistoryRequest
{
    public Guid? SpaceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? BillingPeriodId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class GetUserConsumptionHistoryEndpoint : Endpoint<GetUserConsumptionHistoryRequest, ConsumptionHistoryDto>
{
    private readonly IMediator _mediator;

    public GetUserConsumptionHistoryEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/me/consumption/history");
    }

    public override async Task HandleAsync(GetUserConsumptionHistoryRequest req, CancellationToken ct)
    {
        var query = new GetUserConsumptionHistoryQuery(
            req.SpaceId,
            req.StartDate,
            req.EndDate,
            req.BillingPeriodId,
            req.PageNumber,
            req.PageSize
        );

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendResultAsync(TypedResults.Problem(
                detail: result.Errors.Any() ? result.Errors.First().Message : "Failed to retrieve consumption history",
                statusCode: 400
            ));
        }
    }
}
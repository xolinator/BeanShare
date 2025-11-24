using BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
using BeanShare.Contracts.Analytics;
using BeanShare.Domain.ValueObjects;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Analytics;

public sealed class GetSpaceAnalyticsEndpoint : Endpoint<GetSpaceAnalyticsRequest, GetSpaceAnalyticsResponse>
{
    private readonly IMediator _mediator;

    public GetSpaceAnalyticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{spaceId}/analytics");
        Summary(s =>
        {
            s.Summary = "Get space analytics";
            s.Description = "Retrieve analytics and statistics for a specific coffee space";
        });
    }

    public override async Task HandleAsync(GetSpaceAnalyticsRequest req, CancellationToken ct)
    {
        var spaceIdRaw = Route<string>("spaceId");

        if (!Guid.TryParse(spaceIdRaw, out var spaceGuid))
        {
            AddError("spaceId", "Invalid space ID format");
            await SendErrorsAsync();
            return;
        }

        var spaceId = new SpaceId(spaceGuid);
        var query = new GetSpaceAnalyticsQuery(spaceId, req.FromDate, req.ToDate);
        var result = await _mediator.Send(query, ct);

        var response = new GetSpaceAnalyticsResponse
        {
            TotalMembers = result.TotalMembers,
            TotalConsumptionsThisMonth = result.TotalConsumptionsThisMonth,
            TotalConsumptionsAllTime = result.TotalConsumptionsAllTime,
            TopConsumers = result.TopConsumers.Select(tc => new Contracts.Analytics.TopConsumerDto
            {
                UserId = tc.UserId,
                UserName = tc.UserName,
                CupCount = tc.CupCount,
                TotalCost = tc.TotalCost?.Amount
            }).ToList(),
            PopularCoffeeTypes = result.PopularCoffeeTypes.Select(pc => new Contracts.Analytics.PopularCoffeeDto
            {
                CoffeeName = pc.CoffeeName,
                ConsumptionCount = pc.ConsumptionCount,
                TotalGrams = pc.TotalGrams,
                Percentage = pc.Percentage
            }).ToList(),
            CurrentStockValue = result.CurrentStockValue?.Amount,
            CurrentStockGrams = result.CurrentStockGrams,
            DailyConsumptionTrend = result.DailyConsumptionTrend,
            HourlyConsumptionPattern = result.HourlyConsumptionPattern,
            TotalCostThisMonth = result.TotalCostThisMonth?.Amount
        };

        await SendOkAsync(response, ct);
    }
}
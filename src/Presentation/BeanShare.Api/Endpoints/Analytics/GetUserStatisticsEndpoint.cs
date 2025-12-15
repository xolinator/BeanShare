using BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;
using BeanShare.Contracts.Analytics.UserStatistics;
using BeanShare.Domain.Common;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Analytics;

public sealed class GetUserStatisticsEndpoint : Endpoint<GetUserStatisticsRequest, GetUserStatisticsResponse>
{
    private readonly IMediator _mediator;

    public GetUserStatisticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/users/{userId}/statistics");
        Summary(s =>
        {
            s.Summary = "Get user statistics";
            s.Description = "Retrieve consumption statistics for a specific user";
        });
    }

    public override async Task HandleAsync(GetUserStatisticsRequest req, CancellationToken ct)
    {
        var userIdRaw = Route<string>("userId");

        if (!Guid.TryParse(userIdRaw, out var userGuid))
        {
            AddError("userId", "Invalid user ID format");
            await SendErrorsAsync();
            return;
        }

        var userId = new UserId(userGuid);
        var query = new GetUserStatisticsQuery(userId, req.FromDate, req.ToDate);
        var result = await _mediator.Send(query, ct);

        var response = new GetUserStatisticsResponse
        {
            TotalCups = result.TotalCups,
            CupsThisMonth = result.CupsThisMonth,
            CupsThisWeek = result.CupsThisWeek,
            CupsToday = result.CupsToday,
            AverageCupsPerDay = result.AverageCupsPerDay,
            TotalCost = result.TotalCost?.Amount,
            TotalCostCurrency = result.TotalCost?.Currency.Code,
            IsCostFullyConverted = result.IsCostFullyConverted,
            CostBreakdown = result.CostBreakdown.Select(b => new CurrencyBreakdownResponse
            {
                CurrencyCode = b.CurrencyCode,
                OriginalAmount = b.OriginalAmount,
                ConvertedAmount = b.ConvertedAmount?.Amount,
                ConvertedCurrency = b.ConvertedAmount?.Currency.Code,
                WasConverted = b.WasConverted
            }).ToList(),
            PreferredCurrencyCode = result.PreferredCurrencyCode,
            MostConsumedCoffeeType = result.MostConsumedCoffeeType,
            FirstConsumptionDate = result.FirstConsumptionDate,
            LastConsumptionDate = result.LastConsumptionDate,
            CoffeeTypeBreakdown = result.CoffeeTypeBreakdown,
            DailyConsumptionTrend = result.DailyConsumptionTrend,
            ActiveSpacesCount = result.ActiveSpacesCount
        };

        await SendOkAsync(response, ct);
    }
}
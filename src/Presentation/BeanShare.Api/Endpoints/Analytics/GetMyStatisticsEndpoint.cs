using BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;
using BeanShare.Contracts.Analytics;
using BeanShare.Domain.Common;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Analytics;

public sealed class GetMyStatisticsEndpoint : Endpoint<GetUserStatisticsRequest, GetUserStatisticsResponse>
{
    private readonly IMediator _mediator;

    public GetMyStatisticsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/me/statistics");
        Summary(s =>
        {
            s.Summary = "Get my statistics";
            s.Description = "Retrieve consumption statistics for the current authenticated user";
        });
    }

    public override async Task HandleAsync(GetUserStatisticsRequest req, CancellationToken ct)
    {
        var userIdHeader = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(userIdHeader) || !Guid.TryParse(userIdHeader, out var userGuid))
        {
            AddError("Authentication", "User not authenticated");
            await SendErrorsAsync(401);
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
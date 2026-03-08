using BeanShare.Application.Features.Consumption.Queries;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Consumption;

public sealed class GetRecentConsumptionsRequest
{
    public Guid SpaceId { get; set; }
}

public sealed class GetRecentConsumptionsEndpoint(IMediator mediator)
    : Endpoint<GetRecentConsumptionsRequest, GetRecentConsumptionsResult>
{
    public override void Configure()
    {
        Get("/api/spaces/{SpaceId}/consumption");
        Summary(s =>
        {
            s.Summary = "Get recent consumptions for a space";
            s.Description = "Returns recent consumption entries, user KPIs, and remaining stock for a space.";
        });
    }

    public override async Task HandleAsync(GetRecentConsumptionsRequest req, CancellationToken ct)
    {
        var query = new GetRecentConsumptionsQuery(req.SpaceId);
        var result = await mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

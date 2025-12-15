using BeanShare.Application.Features.Notifications.Queries.GetUnreadCount;
using BeanShare.Contracts.Notifications;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Notifications;

public sealed class GetUnreadCountEndpoint : EndpointWithoutRequest<UnreadCountResponse>
{
    private readonly IMediator _mediator;

    public GetUnreadCountEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/me/notifications/unread-count");
        Summary(s =>
        {
            s.Summary = "Get unread notification count";
            s.Description = "Retrieve the count of unread notifications for the current authenticated user";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetUnreadCountQuery();
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            AddError("Notifications", result.Errors.Any() ? result.Errors.First().Message : "Failed to get unread count");
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendOkAsync(new UnreadCountResponse(result.Value), ct);
    }
}

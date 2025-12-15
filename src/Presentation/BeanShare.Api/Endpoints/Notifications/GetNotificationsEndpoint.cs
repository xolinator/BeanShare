using BeanShare.Application.Features.Notifications.Queries.GetUserNotifications;
using BeanShare.Contracts.Notifications;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Notifications;

public sealed class GetNotificationsEndpoint : Endpoint<GetNotificationsRequest, NotificationListResponse>
{
    private readonly IMediator _mediator;

    public GetNotificationsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/me/notifications");
        Summary(s =>
        {
            s.Summary = "Get user notifications";
            s.Description = "Retrieve all notifications for the current authenticated user";
        });
    }

    public override async Task HandleAsync(GetNotificationsRequest req, CancellationToken ct)
    {
        var query = new GetUserNotificationsQuery(req.UnreadOnly);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            AddError("Notifications", result.Errors.Any() ? result.Errors.First().Message : "Failed to get notifications");
            await SendErrorsAsync(400, ct);
            return;
        }

        var response = new NotificationListResponse(
            result.Value!.TotalCount,
            result.Value.UnreadCount,
            result.Value.Notifications.Select(n => new NotificationResponse(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.SpaceId,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt,
                n.ActionUrl
            )).ToList()
        );

        await SendOkAsync(response, ct);
    }
}

using BeanShare.Application.Features.Admin.Users.Queries;
using BeanShare.Contracts.Admin;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Admin.Users;

public sealed class GetUserDetailsEndpoint : EndpointWithoutRequest<AdminUserDetailResponse>
{
    private readonly IMediator _mediator;

    public GetUserDetailsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/admin/users/{userId}");
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "Get user details (Admin)";
            s.Description = "Retrieve detailed information about a specific user";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        var query = new GetUserDetailsQuery(userId);
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var user = result.Value;
        var response = new AdminUserDetailResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PictureUrl = user.PictureUrl,
            Provider = user.Provider.ToString(),
            SystemRole = user.SystemRole.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            DeactivatedAt = user.DeactivatedAt,
            PreferredCurrencyCode = user.PreferredCurrencyCode,
            Memberships = user.Memberships.Select(m => new UserSpaceMembershipItem
            {
                SpaceId = m.SpaceId,
                SpaceName = m.SpaceName,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}

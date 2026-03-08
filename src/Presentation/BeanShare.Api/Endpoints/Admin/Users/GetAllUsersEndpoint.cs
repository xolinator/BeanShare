using BeanShare.Application.Features.Admin.Users.Queries;
using BeanShare.Contracts.Admin;
using BeanShare.Domain.Enums;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Admin.Users;

public sealed class GetAllUsersEndpoint : Endpoint<GetAllUsersRequest, GetAllUsersResponse>
{
    private readonly IMediator _mediator;

    public GetAllUsersEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/admin/users");
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "Get all users (Admin)";
            s.Description = "Retrieve paginated list of all users with filtering";
        });
    }

    public override async Task HandleAsync(GetAllUsersRequest req, CancellationToken ct)
    {
        SystemRole? roleFilter = req.RoleFilter.HasValue ? (SystemRole)req.RoleFilter.Value : null;

        var query = new GetAllUsersQuery(
            req.Page,
            req.PageSize,
            req.SearchTerm,
            roleFilter,
            req.ActiveFilter);

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

        var response = new GetAllUsersResponse
        {
            Users = result.Value.Users.Select(u => new AdminUserItem
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                PictureUrl = u.PictureUrl,
                Provider = u.Provider.ToString(),
                SystemRole = u.SystemRole.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                DeactivatedAt = u.DeactivatedAt,
                SpaceCount = u.SpaceCount
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            Page = result.Value.Page,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages
        };

        await SendOkAsync(response, ct);
    }
}

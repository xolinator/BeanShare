using BeanShare.Application.Abstractions;
using BeanShare.Contracts.Me;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Me;

public sealed class GetCurrentUserEndpoint : EndpointWithoutRequest<CurrentUserResponse>
{
    private readonly IUserContext _userContext;

    public GetCurrentUserEndpoint(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public override void Configure()
    {
        Get("/api/me");
        Summary(s =>
        {
            s.Summary = "Get current authenticated user";
            s.Description = "Returns the currently authenticated user's information from claims. Requires authentication.";
            s.Response<CurrentUserResponse>(200, "Current user information");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new CurrentUserResponse
        {
            UserId = _userContext.CurrentUserId.Value,
            Email = _userContext.Email,
            Roles = _userContext.Roles.ToList()
        };

        await SendOkAsync(response, ct);
    }
}

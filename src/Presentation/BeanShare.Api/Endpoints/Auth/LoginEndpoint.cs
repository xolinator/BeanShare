using BeanShare.Contracts.Auth;
using BeanShare.Infrastructure.Identity;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Auth;

public sealed class LoginEndpoint : Endpoint<LoginRequest, AuthResponse>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginEndpoint(IAuthenticationService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Login with email and password";
            s.Description = "Authenticates user with email and password, returns JWT token";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(req.Email, req.Password);

        if (!result.Success || result.User == null)
        {
            AddError(result.ErrorMessage ?? "Invalid credentials");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, ct);
            return;
        }

        var token = _jwtTokenService.GenerateToken(result.User);

        var response = new AuthResponse
        {
            Token = token,
            UserId = result.User.Id.Value,
            Email = result.User.Email,
            Name = result.User.Name,
            PictureUrl = result.User.PictureUrl,
            Provider = result.User.Provider.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(1440)
        };

        await SendOkAsync(response, ct);
    }
}

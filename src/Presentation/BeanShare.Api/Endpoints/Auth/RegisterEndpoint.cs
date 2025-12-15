using BeanShare.Contracts.Auth;
using BeanShare.Infrastructure.Identity;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Auth;

public sealed class RegisterEndpoint : Endpoint<RegisterRequest, AuthResponse>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterEndpoint(IAuthenticationService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register new user";
            s.Description = "Creates a new user account with email and password, returns JWT token";
        });
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(req.Email, req.Name, req.Password);

        if (!result.Success || result.User == null)
        {
            AddError(result.ErrorMessage ?? "Registration failed");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
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

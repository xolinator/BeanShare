using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using BeanShare.Contracts.Auth;
using BeanShare.Domain.Enums;
using BeanShare.Infrastructure.Identity;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Auth;

public sealed class ExternalAuthEndpoint : Endpoint<ExternalAuthRequest, AuthResponse>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalAuthEndpoint(
        IAuthenticationService authService,
        IJwtTokenService jwtTokenService,
        IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _httpClientFactory = httpClientFactory;
    }

    public override void Configure()
    {
        Post("/api/auth/external");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Authenticate with external provider";
            s.Description = "Exchanges OAuth ID token for a BeanShare JWT token";
        });
    }

    public override async Task HandleAsync(ExternalAuthRequest req, CancellationToken ct)
    {
        ClaimsPrincipal? principal = null;
        AuthenticationProvider provider;

        try
        {
            if (req.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                provider = AuthenticationProvider.Google;
                principal = await ValidateGoogleTokenAsync(req.IdToken, ct);
            }
            else if (req.Provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
            {
                provider = AuthenticationProvider.Facebook;
                principal = await ValidateFacebookTokenAsync(req.AccessToken ?? req.IdToken, ct);
            }
            else
            {
                AddError("Unsupported provider");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            if (principal == null)
            {
                AddError("Invalid token");
                await SendErrorsAsync(StatusCodes.Status401Unauthorized, ct);
                return;
            }

            var user = await _authService.GetOrCreateUserAsync(principal, provider);

            var token = _jwtTokenService.GenerateToken(user);

            var response = new AuthResponse
            {
                Token = token,
                UserId = user.Id.Value,
                Email = user.Email,
                Name = user.Name,
                PictureUrl = user.PictureUrl,
                Provider = user.Provider.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(1440)
            };

            await SendOkAsync(response, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "External authentication failed");
            AddError("Authentication failed");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, ct);
        }
    }

    private async Task<ClaimsPrincipal?> ValidateGoogleTokenAsync(string idToken, CancellationToken ct)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(idToken);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? ""),
                new Claim(ClaimTypes.Email, jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? ""),
                new Claim(ClaimTypes.Name, jsonToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? ""),
                new Claim("picture", jsonToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value ?? "")
            };

            var identity = new ClaimsIdentity(claims, "Google");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ClaimsPrincipal?> ValidateFacebookTokenAsync(string accessToken, CancellationToken ct)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(
                $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}",
                ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(ct);
            var fbUser = JsonSerializer.Deserialize<FacebookUserInfo>(content);

            if (fbUser == null)
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, fbUser.Id ?? ""),
                new Claim(ClaimTypes.Email, fbUser.Email ?? ""),
                new Claim(ClaimTypes.Name, fbUser.Name ?? ""),
                new Claim("picture", fbUser.Picture?.Data?.Url ?? "")
            };

            var identity = new ClaimsIdentity(claims, "Facebook");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    private class FacebookUserInfo
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public string? Url { get; set; }
    }
}

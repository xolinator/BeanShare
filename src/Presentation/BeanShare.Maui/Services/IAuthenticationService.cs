namespace BeanShare.Maui.Services;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string name, string password);
    Task<AuthResult> LoginWithGoogleAsync();
    Task<AuthResult> LoginWithFacebookAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UserInfo?> GetCurrentUserAsync();
}

public record AuthResult(bool Success, UserInfo? User, string? ErrorMessage);

public record UserInfo(Guid Id, string Email, string Name, string? PictureUrl = null);

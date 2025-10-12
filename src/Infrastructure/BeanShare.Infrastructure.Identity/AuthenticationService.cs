using System.Security.Claims;
using BeanShare.Domain.Entities;
using BeanShare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Identity;

public interface IAuthenticationService
{
    Task<User> GetOrCreateUserAsync(ClaimsPrincipal externalPrincipal, string provider);
    Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(string email, string name, string password);
    Task<(bool Success, User? User, string? ErrorMessage)> LoginAsync(string email, string password);
    Task SignInAsync(HttpContext httpContext, User user, AuthenticationProperties? properties = null);
    Task SignOutAsync(HttpContext httpContext);
}

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly BeanShareDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(BeanShareDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> GetOrCreateUserAsync(ClaimsPrincipal externalPrincipal, string provider)
    {
        var providerUserId = externalPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? externalPrincipal.FindFirstValue("sub")
                          ?? throw new InvalidOperationException("Provider user ID not found in claims");

        var email = externalPrincipal.FindFirstValue(ClaimTypes.Email)
                  ?? externalPrincipal.FindFirstValue("email")
                  ?? throw new InvalidOperationException("Email not found in claims");

        var name = externalPrincipal.FindFirstValue(ClaimTypes.Name)
                ?? externalPrincipal.FindFirstValue("name")
                ?? email.Split('@')[0];

        var pictureUrl = externalPrincipal.FindFirstValue("picture");

        // Try to find existing user by provider and provider user ID
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderUserId == providerUserId);

        if (existingUser != null)
        {
            // Update last login and profile info
            existingUser.UpdateLastLogin();
            existingUser.UpdateProfile(name, pictureUrl);
            await _dbContext.SaveChangesAsync();
            return existingUser;
        }

        // Create new user
        var newUser = User.CreateWithProvider(email, name, provider, providerUserId, pictureUrl);
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return newUser;
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(string email, string name, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
            return (false, null, "Email is required");

        if (string.IsNullOrWhiteSpace(name))
            return (false, null, "Name is required");

        if (string.IsNullOrWhiteSpace(password))
            return (false, null, "Password is required");

        if (password.Length < 8)
            return (false, null, "Password must be at least 8 characters");

        // Check if user already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
            return (false, null, "An account with this email already exists");

        // Hash the password
        var passwordHash = _passwordHasher.HashPassword(password);

        // Create new user
        var user = User.CreateWithPassword(email, name, passwordHash);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return (true, user, null);
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
            return (false, null, "Email is required");

        if (string.IsNullOrWhiteSpace(password))
            return (false, null, "Password is required");

        // Find user by email
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Provider == "Email");

        if (user == null)
            return (false, null, "Invalid email or password");

        // Verify password
        if (user.PasswordHash == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return (false, null, "Invalid email or password");

        // Update last login
        user.UpdateLastLogin();
        await _dbContext.SaveChangesAsync();

        return (true, user, null);
    }

    public async Task SignInAsync(HttpContext httpContext, User user, AuthenticationProperties? properties = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new Claim("sub", user.Id.Value.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("email", user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("name", user.Name),
            new Claim("provider", user.Provider),
        };

        if (!string.IsNullOrEmpty(user.PictureUrl))
        {
            claims.Add(new Claim("picture", user.PictureUrl));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        properties ??= new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    public async Task SignOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

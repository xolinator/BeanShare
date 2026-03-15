using System.Security.Claims;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BeanShare.Application.Services;

/// <summary>
/// Service for synchronizing user data from an OIDC provider to the local database.
/// Creates or updates user records based on OIDC claims.
/// </summary>
public interface IUserSynchronizationService
{
    /// <summary>
    /// Synchronizes a user from OIDC claims to the local database.
    /// Creates the user if they don't exist, or updates their last login time.
    /// </summary>
    Task<User> SyncFromClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed class UserSynchronizationService : IUserSynchronizationService
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<UserSynchronizationService> _logger;

    public UserSynchronizationService(
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<UserSynchronizationService> logger)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<User> SyncFromClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("User is not authenticated");
        }

        // Extract OIDC user ID (sub claim)
        var subClaim = principal.FindFirst("sub")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var oidcUserId))
        {
            throw new InvalidOperationException($"Invalid or missing sub claim: {subClaim}");
        }

        var userId = new UserId(oidcUserId);

        // Check if user already exists by OIDC provider UUID
        var existingUser = await _userService.GetByIdForUpdateAsync(userId, cancellationToken);

        if (existingUser is not null)
        {
            // Update last login time
            existingUser.UpdateLastLogin(_clock.UtcNow);
            await _userService.UpdateAsync(existingUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Updated last login for existing user {UserId}", userId);
            return existingUser;
        }

        // Fallback: check by email (handles seeded users with different IDs than the OIDC provider)
        var email = principal.FindFirst("email")?.Value
            ?? principal.FindFirst("preferred_username")?.Value;

        if (!string.IsNullOrEmpty(email))
        {
            var userByEmail = await _userService.GetByEmailAsync(email, cancellationToken);
            if (userByEmail is not null)
            {
                userByEmail.UpdateLastLogin(_clock.UtcNow);
                await _userService.UpdateAsync(userByEmail, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Matched OIDC user {OidcId} to existing DB user {DbUserId} by email {Email}",
                    oidcUserId, userByEmail.Id, email);
                return userByEmail;
            }
        }

        // Create new user from OIDC claims
        email ??= principal.FindFirst("email")?.Value
            ?? principal.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email claim not found");

        var name = principal.FindFirst("name")?.Value
            ?? principal.FindFirst("given_name")?.Value
            ?? email.Split('@')[0]; // Fallback to email prefix

        var pictureUrl = principal.FindFirst("picture")?.Value;

        // Determine authentication provider from identity_provider claim (for social logins brokered through the OIDC provider)
        var identityProvider = principal.FindFirst("identity_provider")?.Value ?? "oidc";
        var provider = identityProvider.ToLowerInvariant() switch
        {
            "google" => AuthenticationProvider.Google,
            "facebook" => AuthenticationProvider.Facebook,
            _ => AuthenticationProvider.Oidc
        };

        var newUser = User.CreateFromOidc(oidcUserId, email, name, provider, _clock.UtcNow, pictureUrl);

        await _userService.AddAsync(newUser, cancellationToken);

        _logger.LogInformation("Created new user {UserId} ({Email}) from OIDC provider", userId, email);
        return newUser;
    }
}

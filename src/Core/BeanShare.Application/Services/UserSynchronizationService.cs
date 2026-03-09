using System.Security.Claims;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BeanShare.Application.Services;

/// <summary>
/// Service for synchronizing user data from Keycloak to the local database.
/// Creates or updates user records based on OIDC claims.
/// </summary>
public interface IUserSynchronizationService
{
    /// <summary>
    /// Synchronizes a user from Keycloak claims to the local database.
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

        // Extract Keycloak user ID (sub claim)
        var subClaim = principal.FindFirst("sub")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var keycloakUserId))
        {
            throw new InvalidOperationException($"Invalid or missing sub claim: {subClaim}");
        }

        var userId = new UserId(keycloakUserId);

        // Check if user already exists by Keycloak UUID
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

        // Fallback: check by email (handles seeded users with different IDs than Keycloak)
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

                _logger.LogInformation("Matched Keycloak user {KeycloakId} to existing DB user {DbUserId} by email {Email}",
                    keycloakUserId, userByEmail.Id, email);
                return userByEmail;
            }
        }

        // Create new user from Keycloak claims
        email ??= principal.FindFirst("email")?.Value
            ?? principal.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email claim not found");

        var name = principal.FindFirst("name")?.Value
            ?? principal.FindFirst("given_name")?.Value
            ?? email.Split('@')[0]; // Fallback to email prefix

        var pictureUrl = principal.FindFirst("picture")?.Value;

        // Determine authentication provider from identity_provider claim (for social logins brokered through Keycloak)
        var identityProvider = principal.FindFirst("identity_provider")?.Value ?? "keycloak";
        var provider = identityProvider.ToLowerInvariant() switch
        {
            "google" => AuthenticationProvider.Google,
            "facebook" => AuthenticationProvider.Facebook,
            _ => AuthenticationProvider.Keycloak
        };

        var newUser = User.CreateFromKeycloak(keycloakUserId, email, name, provider, _clock.UtcNow, pictureUrl);

        await _userService.AddAsync(newUser, cancellationToken);

        _logger.LogInformation("Created new user {UserId} ({Email}) from Keycloak", userId, email);
        return newUser;
    }
}

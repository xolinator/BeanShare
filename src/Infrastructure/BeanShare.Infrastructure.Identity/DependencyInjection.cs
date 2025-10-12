using BeanShare.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BeanShare.Infrastructure.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, OidcUserContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        })

        // Add Google OAuth
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";

            options.CallbackPath = "/signin-google";
            options.SaveTokens = true;

            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
            options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            options.ClaimActions.MapJsonKey("picture", "picture");

            options.Scope.Add("profile");
            options.Scope.Add("email");
        })

        // Add Facebook OAuth
        .AddFacebook(options =>
        {
            options.AppId = configuration["Authentication:Facebook:AppId"] ?? "";
            options.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? "";

            options.CallbackPath = "/signin-facebook";
            options.SaveTokens = true;

            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            options.ClaimActions.MapJsonKey("picture", "picture");

            options.Scope.Add("email");
            options.Scope.Add("public_profile");

            options.Fields.Add("name");
            options.Fields.Add("email");
            options.Fields.Add("picture");
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

        return services;
    }
}
using BeanShare.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BeanShare.BlazorWeb.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account");

        group.MapPost("/external-login", ExternalLogin);
        group.MapGet("/external-callback", ExternalCallback);
        group.MapPost("/logout", Logout);

        return endpoints;
    }

    private static IResult ExternalLogin(
        [FromForm] string provider,
        [FromForm] string? returnUrl,
        HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(provider) ||
            (provider != "Google" && provider != "Facebook"))
        {
            return Results.BadRequest("Invalid provider");
        }

        var redirectUrl = $"/account/external-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/spaces")}";

        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Results.Challenge(properties, new[] { provider });
    }

    private static async Task<IResult> ExternalCallback(
        HttpContext httpContext,
        [FromQuery] string? returnUrl,
        [FromServices] Infrastructure.Identity.IAuthenticationService authService)
    {
        try
        {
            var result = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Succeeded != true)
            {
                var externalResult = await httpContext.AuthenticateAsync();

                if (externalResult?.Succeeded != true || externalResult.Principal == null)
                {
                    return Results.Redirect("/login?error=authentication_failed");
                }

                var provider = externalResult.Ticket?.AuthenticationScheme ?? "Unknown";

                var user = await authService.GetOrCreateUserAsync(externalResult.Principal, provider);

                await authService.SignInAsync(httpContext, user);
            }

            var finalReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/spaces" : returnUrl;
            return Results.Redirect(finalReturnUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"External callback error: {ex.Message}");
            return Results.Redirect("/login?error=callback_failed");
        }
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        [FromServices] Infrastructure.Identity.IAuthenticationService authService)
    {
        await authService.SignOutAsync(httpContext);
        return Results.Redirect("/");
    }
}

using BeanShare.BlazorWeb.Components;
using BeanShare.BlazorWeb.Endpoints;
using BeanShare.Application;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence.Seeds;
using BeanShare.SharedUi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=beanshare_dev;Username=beanshare;Password=beanshare123";

var useKeycloak = builder.Configuration.GetValue<bool>("UseKeycloak", false);
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: useInMemoryDatabase);
builder.Services.AddExchangeRates(builder.Configuration);
builder.Services.AddCommunicationServices(builder.Configuration);

if (useKeycloak)
{
    // Keycloak OIDC configuration
    var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
        ?? throw new InvalidOperationException("Keycloak:Authority not configured");
    var keycloakClientId = builder.Configuration["Keycloak:ClientId"] ?? "beanshare-web";
    var keycloakClientSecret = builder.Configuration["Keycloak:ClientSecret"]
        ?? throw new InvalidOperationException("Keycloak:ClientSecret not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = keycloakAuthority;
        options.ClientId = keycloakClientId;
        options.ClientSecret = keycloakClientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.MapInboundClaims = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                var tokenLogger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // Extract realm roles from Keycloak token
                if (context.Principal != null)
                {
                    var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                    if (identity != null)
                    {
                        tokenLogger.LogInformation("=== KEYCLOAK TOKEN VALIDATION ===");
                        tokenLogger.LogInformation("All claims before role extraction:");
                        foreach (var claim in identity.Claims)
                        {
                            tokenLogger.LogInformation("  Claim Type: {Type}, Value: {Value}", claim.Type,
                                claim.Type == "realm_access" ? "[JSON]" : claim.Value);
                        }

                        var realmAccessClaim = identity.FindFirst("realm_access");
                        if (realmAccessClaim != null)
                        {
                            try
                            {
                                tokenLogger.LogInformation("Found realm_access claim, parsing JSON...");
                                var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                                {
                                    tokenLogger.LogInformation("Found roles array in realm_access");
                                    foreach (var role in rolesElement.EnumerateArray())
                                    {
                                        var roleValue = role.GetString() ?? "";
                                        tokenLogger.LogInformation("  Adding role claim: {Role}", roleValue);
                                        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleValue));
                                    }
                                }
                                else
                                {
                                    tokenLogger.LogWarning("No 'roles' property found in realm_access JSON");
                                }
                            }
                            catch (Exception ex)
                            {
                                tokenLogger.LogError(ex, "Failed to parse realm_access claim");
                            }
                        }
                        else
                        {
                            tokenLogger.LogWarning("No realm_access claim found in token");
                        }

                        tokenLogger.LogInformation("All claims after role extraction:");
                        foreach (var claim in identity.Claims)
                        {
                            if (claim.Type == System.Security.Claims.ClaimTypes.Role)
                            {
                                tokenLogger.LogInformation("  ROLE CLAIM: {Value}", claim.Value);
                            }
                        }
                        tokenLogger.LogInformation("=== END TOKEN VALIDATION ===");
                    }
                }

                var syncService = context.HttpContext.RequestServices
                    .GetService<BeanShare.Application.Services.IUserSynchronizationService>();
                if (syncService != null && context.Principal != null)
                {
                    try
                    {
                        await syncService.SyncFromClaimsAsync(context.Principal);
                    }
                    catch (Exception ex)
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "Failed to sync user from Keycloak claims");
                    }
                }
            }
        };
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, KeycloakUserContext>();
    builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, BeanShare.Application.Services.UserSynchronizationService>();
}
else
{
    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, BeanShare.BlazorWeb.Services.HttpUserContext>();
}

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient("BeanShareApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5247");
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BeanShareApi"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IThemeService, ThemeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseHttpsRedirection();

app.UseAuthentication();

// Development auto-login: automatically sign in as seeded user "John Smith"
// This avoids needing to manually log in when using InMemory database with seeded data
if (app.Environment.IsDevelopment() && !useKeycloak)
{
    app.Use(async (context, next) =>
    {
        if (context.User?.Identity?.IsAuthenticated != true
            && !context.Request.Path.StartsWithSegments("/login")
            && !context.Request.Path.StartsWithSegments("/register")
            && !context.Request.Path.StartsWithSegments("/account")
            && !context.Request.Path.StartsWithSegments("/_framework")
            && !context.Request.Path.StartsWithSegments("/_content")
            && !context.Request.Path.StartsWithSegments("/_blazor"))
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111"),
                new("sub", "11111111-1111-1111-1111-111111111111"),
                new(System.Security.Claims.ClaimTypes.Email, "john.smith@beanshare.dev"),
                new("email", "john.smith@beanshare.dev"),
                new(System.Security.Claims.ClaimTypes.Name, "John Smith"),
                new("name", "John Smith"),
                new(System.Security.Claims.ClaimTypes.Role, "Admin"),
                new("provider", "Email"),
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });

            context.User = principal;
        }
        await next();
    });
}

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BeanShare.Infrastructure.Persistence.BeanShareDbContext>();
        await context.Database.EnsureCreatedAsync();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();
    }
}

app.UseAntiforgery();

app.MapStaticAssets();

app.MapAccountEndpoints();
app.MapSettlementExportEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using BeanShare.BlazorWeb.Components;
using BeanShare.BlazorWeb.Endpoints;
using BeanShare.Application;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence;
using BeanShare.Infrastructure.Persistence.Seeds;
using BeanShare.Infrastructure.Services;
using BeanShare.SharedUi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=beanshare_dev;Username=beanshare;Password=beanshare123";

var useOidc = builder.Configuration.GetValue<bool>("UseOidc", false);
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: useInMemoryDatabase);
builder.Services.AddMemoryCache();
builder.Services.AddExchangeRates(builder.Configuration);
builder.Services.AddCommunicationServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

if (!useInMemoryDatabase)
{
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<BeanShareDbContext>();
}

if (useOidc)
{
    // OIDC configuration
    var oidcAuthority = builder.Configuration["Oidc:Authority"]
        ?? throw new InvalidOperationException("Oidc:Authority not configured");
    var oidcClientId = builder.Configuration["Oidc:ClientId"] ?? "beanshare-web";
    var oidcClientSecret = builder.Configuration["Oidc:ClientSecret"]
        ?? throw new InvalidOperationException("Oidc:ClientSecret not configured");

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
        options.Authority = oidcAuthority;
        options.ClientId = oidcClientId;
        options.ClientSecret = oidcClientSecret;
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
                // Extract roles from multiple OIDC claim formats for provider compatibility
                if (context.Principal != null)
                {
                    var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                    if (identity != null)
                    {
                        // Keycloak: realm_access JSON with nested roles array
                        var realmAccessClaim = identity.FindFirst("realm_access");
                        if (realmAccessClaim != null)
                        {
                            try
                            {
                                var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                                {
                                    foreach (var role in rolesElement.EnumerateArray())
                                    {
                                        var roleValue = role.GetString() ?? "";
                                        identity.AddClaim(new System.Security.Claims.Claim(
                                            System.Security.Claims.ClaimTypes.Role, roleValue));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                logger.LogError(ex, "Failed to parse realm_access claim");
                            }
                        }

                        var rolesClaim = identity.FindFirst("roles");
                        if (rolesClaim != null && rolesClaim.Value.TrimStart().StartsWith("["))
                        {
                            try
                            {
                                using var rolesDoc = System.Text.Json.JsonDocument.Parse(rolesClaim.Value);
                                foreach (var role in rolesDoc.RootElement.EnumerateArray())
                                {
                                    var roleValue = role.GetString() ?? "";
                                    identity.AddClaim(new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, roleValue));
                                }
                            }
                            catch (Exception) { }
                        }
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
                        logger.LogError(ex, "Failed to sync user from OIDC claims");
                    }
                }
            }
        };
    });

    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, OidcUserContext>();
    builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, BeanShare.Application.Services.UserSynchronizationService>();
    builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, BeanShare.Infrastructure.Identity.OidcClaimsTransformation>();
}
else
{
    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, BeanShare.BlazorWeb.Services.HttpUserContext>();
}

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<BeanShare.SharedUi.Services.IQrCodeService>(sp =>
{
    var qrCodeBaseUrl = ResolvePublicBaseUrl(
        sp,
        builder.Configuration.GetValue<string>("QrCodeBaseUrl"),
        "http://localhost:5126");

    return new BeanShare.SharedUi.Services.QrCodeService(qrCodeBaseUrl);
});
builder.Services.AddSingleton<BeanShare.SharedUi.Services.IQrScannerService, BeanShare.SharedUi.Services.BrowserQrScannerService>();
builder.Services.AddSingleton<BeanShare.SharedUi.Services.QrCodeResolveCache>();

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
#pragma warning disable ASPDEPR005
forwardedHeadersOptions.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseAuthentication();
app.UseAuthorization();

var includeWebDemoData = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("IncludeDemoData");
await app.Services.InitializeBeanShareDatabaseAsync(includeWebDemoData);

app.UseAntiforgery();

var avatarStorage = app.Services.GetRequiredService<AvatarStorageService>();
var uploadsRootPath = avatarStorage.GetUploadsRootPath();
Directory.CreateDirectory(uploadsRootPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRootPath),
    RequestPath = "/uploads"
});
app.UseStaticFiles();
app.MapStaticAssets();

app.MapAccountEndpoints();
app.MapSettlementExportEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string ResolvePublicBaseUrl(IServiceProvider services, string? configuredValue, string developmentFallback)
{
    if (!string.IsNullOrWhiteSpace(configuredValue))
    {
        return configuredValue.TrimEnd('/');
    }

    var navigationManager = services.GetService<NavigationManager>();
    if (navigationManager != null && !string.IsNullOrWhiteSpace(navigationManager.BaseUri))
    {
        return navigationManager.BaseUri.TrimEnd('/');
    }

    var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (httpContext != null)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}".TrimEnd('/');
    }

    return developmentFallback.TrimEnd('/');
}

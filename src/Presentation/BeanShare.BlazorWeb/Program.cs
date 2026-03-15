using BeanShare.BlazorWeb.Components;
using BeanShare.BlazorWeb.Endpoints;
using BeanShare.Application;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence;
using BeanShare.Infrastructure.Persistence.Seeds;
using BeanShare.SharedUi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddExchangeRates(builder.Configuration);
builder.Services.AddCommunicationServices(builder.Configuration);

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
                // Extract realm roles from OIDC token (e.g. Keycloak realm_access claim)
                if (context.Principal != null)
                {
                    var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                    if (identity != null)
                    {
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

    builder.Services.AddHttpContextAccessor();
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

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrEmpty(apiBaseUrl))
    apiBaseUrl = "http://localhost:5247";

builder.Services.AddHttpClient("BeanShareApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BeanShareApi"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IThemeService, ThemeService>();

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

// Ensure database schema exists and seed data
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BeanShareDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Create DataProtectionKeys table if it doesn't exist (EnsureCreated won't add new tables to existing DB)
    if (!useInMemoryDatabase)
    {
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
                "Id" SERIAL PRIMARY KEY,
                "FriendlyName" TEXT NULL,
                "Xml" TEXT NULL
            )
            """);
    }

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
    var seeder = new DatabaseSeeder(context, logger);
    await seeder.SeedAsync(includeDemoData: true);
}

app.UseAntiforgery();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapAccountEndpoints();
app.MapSettlementExportEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using BeanShare.Application;
using BeanShare.Application.Constants;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence;
using BeanShare.Infrastructure.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddApplication();

builder.Services.AddMapster();

var useOidc = builder.Configuration.GetValue<bool>("UseOidc", false);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: useInMemoryDatabase);
builder.Services.AddMemoryCache();
builder.Services.AddExchangeRates(builder.Configuration);
builder.Services.AddCommunicationServices(builder.Configuration);

builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, BeanShare.Application.Services.UserSynchronizationService>();

if (useOidc)
{
    // OIDC configuration
    var oidcAuthority = builder.Configuration["Oidc:Authority"]
        ?? throw new InvalidOperationException("Oidc:Authority not configured");
    var oidcAudience = builder.Configuration["Oidc:Audience"] ?? "beanshare-api";

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, OidcUserContext>();
    builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, BeanShare.Infrastructure.Identity.OidcClaimsTransformation>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = oidcAuthority;
            options.Audience = oidcAudience;
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    oidcAuthority,
                    // Android emulator uses 10.0.2.2 to reach host, so tokens have a different issuer
                    oidcAuthority.Replace("localhost", "10.0.2.2"),
                },
                // Public OIDC clients (beanshare-mobile) may not include an audience claim.
                // Issuer validation is sufficient since all clients share the same provider.
                ValidateAudience = false,
                ValidateLifetime = true,
                NameClaimType = "preferred_username",
                ClockSkew = TimeSpan.FromMinutes(AuthenticationSettings.TokenClockSkewMinutes)
            };

            // Extract roles from multiple OIDC claim formats for provider compatibility.
            // Keycloak: realm_access.roles (JSON), Auth0/Azure AD: roles (array), standard: role claim.
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    if (identity == null) return Task.CompletedTask;

                    // Keycloak: realm_access JSON with nested roles array
                    var realmAccessClaim = identity.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccessClaim))
                    {
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(realmAccessClaim);
                            if (doc.RootElement.TryGetProperty("roles", out var rolesElement) &&
                                rolesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrWhiteSpace(roleName))
                                        identity.AddClaim(new System.Security.Claims.Claim(
                                            System.Security.Claims.ClaimTypes.Role, roleName));
                                }
                            }
                        }
                        catch (System.Text.Json.JsonException) { }
                    }

                    // Auth0 / Azure AD / generic: "roles" claim as JSON array
                    var rolesClaim = identity.FindFirst("roles")?.Value;
                    if (!string.IsNullOrEmpty(rolesClaim) && rolesClaim.TrimStart().StartsWith("["))
                    {
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(rolesClaim);
                            foreach (var role in doc.RootElement.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (!string.IsNullOrWhiteSpace(roleName))
                                    identity.AddClaim(new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, roleName));
                            }
                        }
                        catch (System.Text.Json.JsonException) { }
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    });
}
else
{
    builder.Services.AddIdentityInfrastructure(builder.Configuration);

    var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BeanShare";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BeanShare";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.FromMinutes(AuthenticationSettings.TokenClockSkewMinutes)
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    });
}

builder.Services.SwaggerDocument();

var app = builder.Build();

var fhOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
#pragma warning disable ASPDEPR005
fhOptions.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
fhOptions.KnownProxies.Clear();
app.UseForwardedHeaders(fhOptions);

var avatarStorage = app.Services.GetRequiredService<AvatarStorageService>();
var uploadsRootPath = avatarStorage.GetUploadsRootPath();
Directory.CreateDirectory(uploadsRootPath);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRootPath),
    RequestPath = "/uploads"
});
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Serializer.Options.PropertyNamingPolicy = null;
});

var includeApiDemoData = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("IncludeDemoData");
await app.Services.InitializeBeanShareDatabaseAsync(includeApiDemoData);

// Validate critical configuration on startup
ValidateConfiguration(app.Configuration, useOidc, app.Logger);

app.Run();

static void ValidateConfiguration(IConfiguration configuration, bool useOidc, ILogger logger)
{
    var warnings = new List<string>();

    // Check OpenExchangeRates API key
    var openExchangeRatesAppId = configuration.GetValue<string>("OpenExchangeRates:AppId");
    if (string.IsNullOrWhiteSpace(openExchangeRatesAppId))
    {
        warnings.Add("OpenExchangeRates:AppId is not configured. Currency conversion will not work. Get a free API key from https://openexchangerates.org/signup/free");
    }

    // Check database connection
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogError("CRITICAL: Database connection string is not configured!");
    }

    // Check OIDC configuration
    if (useOidc)
    {
        var oidcAuthority = configuration.GetValue<string>("Oidc:Authority");
        if (string.IsNullOrWhiteSpace(oidcAuthority))
        {
            logger.LogError("CRITICAL: Oidc:Authority is not configured but UseOidc is true!");
        }
    }
    else
    {
        // Check JWT secret (only needed when not using OIDC)
        var jwtSecret = configuration.GetValue<string>("Jwt:Secret");
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            logger.LogError("CRITICAL: JWT:Secret is not configured!");
        }
        else if (jwtSecret.Length < 32)
        {
            logger.LogWarning("JWT:Secret is less than 32 characters. Consider using a longer secret for better security.");
        }
    }

    // Check email configuration
    var emailEnabled = configuration.GetValue<bool>("Email:Enabled", false);
    if (emailEnabled)
    {
        var smtpUsername = configuration.GetValue<string>("Email:SmtpUsername");
        var smtpPassword = configuration.GetValue<string>("Email:SmtpPassword");

        if (string.IsNullOrWhiteSpace(smtpUsername) || string.IsNullOrWhiteSpace(smtpPassword))
        {
            warnings.Add("Email service is enabled but credentials are not configured. Email functionality will not work.");
        }
    }

    // Log all warnings
    foreach (var warning in warnings)
    {
        logger.LogWarning("Configuration: {Warning}", warning);
    }

    if (warnings.Count == 0)
    {
        logger.LogInformation("Configuration validation complete. All critical settings are configured.");
    }
}

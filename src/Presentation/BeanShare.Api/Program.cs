using BeanShare.Application;
using BeanShare.Application.Constants;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddApplication();

builder.Services.AddMapster();

var useKeycloak = builder.Configuration.GetValue<bool>("UseKeycloak", false);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: useInMemoryDatabase);
builder.Services.AddExchangeRates(builder.Configuration);
builder.Services.AddCommunicationServices(builder.Configuration);

builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, BeanShare.Application.Services.UserSynchronizationService>();

if (useKeycloak)
{
    // Keycloak OIDC configuration
    var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
        ?? throw new InvalidOperationException("Keycloak:Authority not configured");
    var keycloakAudience = builder.Configuration["Keycloak:Audience"] ?? "beanshare-api";

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, KeycloakUserContext>();
    builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, BeanShare.Infrastructure.Identity.KeycloakClaimsTransformation>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakAuthority;
            options.Audience = keycloakAudience;
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    keycloakAuthority,
                    // Android emulator uses 10.0.2.2 to reach host, so tokens have a different issuer
                    keycloakAuthority.Replace("localhost", "10.0.2.2"),
                },
                // Keycloak public clients (beanshare-mobile) don't include an audience claim by default.
                // Issuer validation is sufficient since all clients are in the same realm.
                ValidateAudience = false,
                ValidateLifetime = true,
                NameClaimType = "preferred_username",
                ClockSkew = TimeSpan.FromMinutes(AuthenticationSettings.TokenClockSkewMinutes)
            };

            // Keycloak puts roles in realm_access as JSON: {"roles":["admin","user"]}
            // ASP.NET Core can't parse nested JSON as role claims, so we extract them manually.
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    var realmAccessClaim = identity?.FindFirst("realm_access")?.Value;
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
                                    {
                                        identity!.AddClaim(new System.Security.Claims.Claim(
                                            System.Security.Claims.ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            // Malformed realm_access claim — skip role extraction
                        }
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

var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Serializer.Options.PropertyNamingPolicy = null;
});

// Seed database: essential data (global presets) in all environments,
// demo data (users, spaces, consumption, etc.) only in development.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BeanShare.Infrastructure.Persistence.BeanShareDbContext>();
    await context.Database.EnsureCreatedAsync();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<BeanShare.Infrastructure.Persistence.Seeds.DatabaseSeeder>>();
    var seeder = new BeanShare.Infrastructure.Persistence.Seeds.DatabaseSeeder(context, logger);
    await seeder.SeedAsync(includeDemoData: true);
}

// Validate critical configuration on startup
ValidateConfiguration(app.Configuration, useKeycloak, app.Logger);

app.Run();

static void ValidateConfiguration(IConfiguration configuration, bool useKeycloak, ILogger logger)
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

    // Check Keycloak configuration
    if (useKeycloak)
    {
        var keycloakAuthority = configuration.GetValue<string>("Keycloak:Authority");
        if (string.IsNullOrWhiteSpace(keycloakAuthority))
        {
            logger.LogError("CRITICAL: Keycloak:Authority is not configured but UseKeycloak is true!");
        }
    }
    else
    {
        // Check JWT secret (only needed when not using Keycloak)
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

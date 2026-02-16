using BeanShare.Api.Infrastructure.Mocks;
using BeanShare.Application;
using BeanShare.Application.Constants;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddApplication();

builder.Services.AddMapster();

var useMockServices = builder.Configuration.GetValue<bool>("UseMockServices", false);
var useMockAuthentication = builder.Configuration.GetValue<bool>("UseMockAuthentication", false);
var useKeycloak = builder.Configuration.GetValue<bool>("UseKeycloak", false);

if (useMockServices)
{
    builder.Services.AddSingleton<BeanShare.Domain.Common.IClock, SystemClock>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IInviteCodeGenerator, MockInviteCodeGenerator>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.ISpaceRepository, MockSpaceRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IUnitOfWork, MockUnitOfWork>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IUserContext, MockUserContext>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IBillingPeriodRepository, MockBillingPeriodRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.ISettlementRepository, MockSettlementRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.ICoffeeStockRepository, MockCoffeeStockRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IConsumptionRepository, MockConsumptionRepository>();
    builder.Services.AddSingleton<BeanShare.Domain.Services.ICostingPolicy, BeanShare.Domain.Services.WeightedAverageCostingPolicy>();
    builder.Services.AddSingleton<BeanShare.Application.Services.IUserService, MockUserService>();
    builder.Services.AddSingleton<BeanShare.Application.Services.ICostCalculationService, MockCostCalculationService>();
    builder.Services.AddScoped<IAuthenticationService, MockIdentityAuthenticationService>();
    builder.Services.AddSingleton<IJwtTokenService, MockJwtTokenService>();
    builder.Services.AddSingleton<BeanShare.Domain.Repositories.IGlobalPresetRepository, MockGlobalPresetRepository>();
    builder.Services.AddSingleton<BeanShare.Domain.Repositories.IPresetRecipeRepository, MockPresetRecipeRepository>();
    builder.Services.AddSingleton<BeanShare.Domain.Repositories.ISpaceGlobalPresetConfigRepository, MockSpaceGlobalPresetConfigRepository>();
    builder.Services.AddSingleton<BeanShare.Domain.Repositories.IUserPresetFavoriteRepository, MockUserPresetFavoriteRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.INotificationRepository, MockNotificationRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IExchangeRateRepository, MockExchangeRateRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IUserRepository, MockUserRepository>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IEmailService, MockEmailService>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.ISettlementEmailTemplateService, MockSettlementEmailTemplateService>();
    builder.Services.AddSingleton<BeanShare.Application.Services.ICurrencyConversionService, MockCurrencyConversionService>();
    builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, MockUserSynchronizationService>();
    builder.Services.AddSingleton<BeanShare.Infrastructure.Services.Documents.ISettlementReportGenerator, MockSettlementReportGenerator>();
    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IExchangeRateProvider, MockExchangeRateProvider>();
    builder.Services.AddHttpClient();
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

    builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: useInMemoryDatabase);
    builder.Services.AddExchangeRates(builder.Configuration);
    builder.Services.AddCommunicationServices(builder.Configuration);

    // Register IUserSynchronizationService for Keycloak user sync endpoint
    builder.Services.AddScoped<BeanShare.Application.Services.IUserSynchronizationService, BeanShare.Application.Services.UserSynchronizationService>();

    if (useMockAuthentication)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, MockUserContext>();
        builder.Services.AddScoped<IAuthenticationService, MockIdentityAuthenticationService>();
        builder.Services.AddSingleton<IJwtTokenService, MockJwtTokenService>();
        builder.Services.AddHttpClient();
    }
    else
    {
        builder.Services.AddIdentityInfrastructure(builder.Configuration);
    }
}

if (useMockServices || useMockAuthentication)
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Mock";
            options.DefaultChallengeScheme = "Mock";
            options.DefaultScheme = "Mock";
        })
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", null);

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes("Mock")
            .RequireAuthenticatedUser()
            .Build();
    });
}
else if (useKeycloak)
{
    // Keycloak OIDC configuration
    var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
        ?? throw new InvalidOperationException("Keycloak:Authority not configured");
    var keycloakAudience = builder.Configuration["Keycloak:Audience"] ?? "beanshare-api";

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, KeycloakUserContext>();

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
                ValidIssuer = keycloakAuthority,
                ValidateAudience = true,
                ValidAudience = keycloakAudience,
                ValidateLifetime = true,
                NameClaimType = "preferred_username",
                RoleClaimType = "realm_access",
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
else
{
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

if (app.Environment.IsDevelopment() && !useMockServices)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<BeanShare.Infrastructure.Persistence.BeanShareDbContext>();
        await context.Database.EnsureCreatedAsync();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BeanShare.Infrastructure.Persistence.Seeds.DatabaseSeeder>>();
        var seeder = new BeanShare.Infrastructure.Persistence.Seeds.DatabaseSeeder(context, logger);
        await seeder.SeedAsync();
    }
}

// Validate critical configuration on startup
ValidateConfiguration(app.Configuration, app.Logger);

app.Run();

static void ValidateConfiguration(IConfiguration configuration, ILogger logger)
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
    var useKeycloak = configuration.GetValue<bool>("UseKeycloak", false);
    if (useKeycloak)
    {
        var keycloakAuthority = configuration.GetValue<string>("Keycloak:Authority");
        var keycloakClientSecret = configuration.GetValue<string>("Keycloak:ClientSecret");

        if (string.IsNullOrWhiteSpace(keycloakAuthority))
        {
            logger.LogError("CRITICAL: Keycloak:Authority is not configured but UseKeycloak is true!");
        }

        if (string.IsNullOrWhiteSpace(keycloakClientSecret))
        {
            warnings.Add("Keycloak:ClientSecret is not configured. This may cause authentication issues.");
        }
    }

    // Check JWT secret
    var jwtSecret = configuration.GetValue<string>("Jwt:Secret");
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        logger.LogError("CRITICAL: JWT:Secret is not configured!");
    }
    else if (jwtSecret.Length < 32)
    {
        logger.LogWarning("JWT:Secret is less than 32 characters. Consider using a longer secret for better security.");
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

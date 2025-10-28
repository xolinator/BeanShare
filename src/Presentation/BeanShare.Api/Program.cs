using BeanShare.Api.Infrastructure.Mocks;
using BeanShare.Application;
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
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddInfrastructure(connectionString);

    if (useMockAuthentication)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, MockUserContext>();
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
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwt = builder.Configuration.GetSection("Authentication:Jwt");
            options.Authority = jwt["Authority"];
            options.Audience = jwt["Audience"];
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.MapInboundClaims = false;
            var metadata = jwt["MetadataAddress"];
            if (!string.IsNullOrWhiteSpace(metadata))
                options.MetadataAddress = metadata;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
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

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Serializer.Options.PropertyNamingPolicy = null;
});

// Seed database in development mode
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

app.Run();

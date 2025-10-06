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
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddInfrastructure(connectionString);

    if (useMockAuthentication)
    {
        builder.Services.AddHttpContextAccessor();
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

app.Run();

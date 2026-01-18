using BeanShare.BlazorWeb.Components;
using BeanShare.BlazorWeb.Endpoints;
using BeanShare.Application;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence.Seeds;
using BeanShare.SharedUi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=beanshare_dev;Username=beanshare;Password=beanshare123";

var useKeycloak = builder.Configuration.GetValue<bool>("UseKeycloak", false);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: false);
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
            RoleClaimType = "realm_access"
        };

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

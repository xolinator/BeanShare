using BeanShare.BlazorWeb.Components;
using BeanShare.BlazorWeb.Endpoints;
using BeanShare.Application;
using BeanShare.Infrastructure;
using BeanShare.Infrastructure.Identity;
using BeanShare.Infrastructure.Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=beanshare_dev;Username=beanshare;Password=beanshare123";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
// Use PostgreSQL database with seeded data
builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase: false);

// Add authentication and authorization
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Add cascading authentication state
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Database seeding is handled by the API on startup
// No need to seed here when using PostgreSQL

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// Map account endpoints for authentication
app.MapAccountEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

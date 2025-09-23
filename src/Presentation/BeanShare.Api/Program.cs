using BeanShare.Api.Infrastructure.Mocks;
using BeanShare.Application.Behaviors;
using BeanShare.Application.Common;
using BeanShare.Infrastructure;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

builder.Services.AddMediatR(typeof(Result).Assembly);
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

builder.Services.AddMapster();

var useMockServices = builder.Configuration.GetValue<bool>("UseMockServices", false);

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

    builder.Services.AddSingleton<BeanShare.Application.Abstractions.IUserContext, MockUserContext>();
}

builder.Services.SwaggerDocument();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Serializer.Options.PropertyNamingPolicy = null;
});

app.Run();

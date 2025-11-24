using Microsoft.Extensions.Logging;
using BeanShare.Maui.Services;
using MediatR;

namespace BeanShare.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddAuthorizationCore();

		builder.Services.AddTransient<AuthenticationHandler>();

		builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5247") });

		builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});
		builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MauiAuthenticationStateProvider>();

		builder.Services.AddHttpClient<ISpacesService, SpacesService>(client =>
		{
			// Android emulator: http://10.0.2.2:5247
			// iOS simulator/Windows: http://localhost:5247
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IStockService, StockService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IConsumptionService, ConsumptionService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IBillingService, BillingService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<ISettlementService, SettlementService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IAnalyticsService, AnalyticsService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, UserContextStub>();
		builder.Services.AddScoped<IMauiUserContext, MauiUserContext>();
		builder.Services.AddScoped<IMediator, MediatorStub>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BeanShare.Maui.Services;
using BeanShare.SharedUi.Services;
using MediatR;
using System.Reflection;

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

		// Load configuration from embedded appsettings.json
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("BeanShare.Maui.appsettings.json");
		if (stream != null)
		{
			var config = new ConfigurationBuilder()
				.AddJsonStream(stream)
				.Build();

			builder.Configuration.AddConfiguration(config);
		}

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddAuthorizationCore();

		builder.Services.AddTransient<AuthenticationHandler>();

		// Get API base URL from configuration with Android emulator detection
		var apiBaseUrl = GetApiBaseUrl(builder.Configuration);

		builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

		builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});
		builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MauiAuthenticationStateProvider>();

		builder.Services.AddHttpClient<ISpacesService, SpacesService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IStockService, StockService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IConsumptionService, ConsumptionService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IBillingService, BillingService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<ISettlementService, SettlementService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IAnalyticsService, AnalyticsService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<IPresetService, PresetService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<AuthenticationHandler>();

		builder.Services.AddScoped<BeanShare.Application.Abstractions.IUserContext, UserContextStub>();
		builder.Services.AddScoped<IMauiUserContext, MauiUserContext>();
		builder.Services.AddScoped<IMediator, MediatorStub>();
		builder.Services.AddSingleton<IAlertService, AlertService>();
		builder.Services.AddScoped<IThemeService, ThemeService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static string GetApiBaseUrl(IConfiguration configuration)
	{
		var apiBaseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5247";

		// Android emulator needs special localhost address
		#if ANDROID
		if (apiBaseUrl.Contains("localhost") || apiBaseUrl.Contains("127.0.0.1"))
		{
			apiBaseUrl = apiBaseUrl.Replace("localhost", "10.0.2.2").Replace("127.0.0.1", "10.0.2.2");
		}
		#endif

		return apiBaseUrl;
	}
}

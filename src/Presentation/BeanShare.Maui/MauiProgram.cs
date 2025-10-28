using Microsoft.Extensions.Logging;
using BeanShare.Maui.Services;

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

		// Add authentication services
		builder.Services.AddAuthorizationCore();
		builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MockAuthenticationStateProvider>();

		// Configure HttpClient for API communication
		builder.Services.AddHttpClient<ISpacesService, SpacesService>(client =>
		{
			// Platform-specific base addresses:
			// Android emulator: http://10.0.2.2:5247
			// iOS simulator/Windows: http://localhost:5247
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		builder.Services.AddHttpClient<IStockService, StockService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		builder.Services.AddHttpClient<IConsumptionService, ConsumptionService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		builder.Services.AddHttpClient<IBillingService, BillingService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		builder.Services.AddHttpClient<ISettlementService, SettlementService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5247");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

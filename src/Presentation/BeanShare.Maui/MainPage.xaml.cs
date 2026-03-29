using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BeanShare.Maui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("[BeanShare] MainPage constructor starting");
			InitializeComponent();
			System.Diagnostics.Debug.WriteLine("[BeanShare] InitializeComponent completed");

			blazorWebView.BlazorWebViewInitializing += OnBlazorWebViewInitializing;
			blazorWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
			blazorWebView.UrlLoading += OnUrlLoading;

			System.Diagnostics.Debug.WriteLine("[BeanShare] MainPage constructor completed");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[BeanShare] ERROR in MainPage constructor: {ex}");
			throw;
		}
	}

	private void OnBlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("[BeanShare] BlazorWebView Initializing...");
#if DEBUG && WINDOWS
		// Enable remote debugging for WebView2 on port 9222
		e.EnvironmentOptions = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions
		{
			AdditionalBrowserArguments = "--remote-debugging-port=9222"
		};
		System.Diagnostics.Debug.WriteLine("[BeanShare] Remote debugging enabled on port 9222");
#endif
	}

	private void OnUrlLoading(object? sender, UrlLoadingEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"[BeanShare] URL Loading: {e.Url}");
		e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
	}

	private void OnBlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("[BeanShare] BlazorWebView Initialized!");
		System.Diagnostics.Debug.WriteLine($"[BeanShare] WebView: {e.WebView}");

#if ANDROID
		if (e.WebView is Android.Webkit.WebView androidWebView)
		{
			androidWebView.Settings.MediaPlaybackRequiresUserGesture = false;
			androidWebView.SetWebChromeClient(new Platforms.Android.CameraWebChromeClient(androidWebView.WebChromeClient));
			System.Diagnostics.Debug.WriteLine("[BeanShare] Camera WebChromeClient configured");
		}
#endif
	}
}

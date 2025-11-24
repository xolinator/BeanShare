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
	}
}

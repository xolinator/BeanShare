using Microsoft.JSInterop;

namespace BeanShare.SharedUi.Services;

public interface IThemeService
{
    Task<string> GetThemeAsync();
    Task SetThemeAsync(string theme);
    Task InitializeAsync();
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetThemeAsync()
    {
        try
        {
            _currentTheme = await _jsRuntime.InvokeAsync<string>("beanshareTheme.getTheme");
            return _currentTheme;
        }
        catch
        {
            return _currentTheme;
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("beanshareTheme.setTheme", theme);
            _currentTheme = theme;
        }
        catch
        {
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            _currentTheme = await _jsRuntime.InvokeAsync<string>("beanshareTheme.init");
        }
        catch
        {
        }
    }
}

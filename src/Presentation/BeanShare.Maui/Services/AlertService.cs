namespace BeanShare.Maui.Services;

public class AlertService : IAlertService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        var page = GetCurrentPage();
        if (page is not null)
        {
#pragma warning disable CS0618 // DisplayAlert is obsolete but DisplayAlertAsync is not available in all MAUI versions
            await page.DisplayAlert(title, message, cancel);
#pragma warning restore CS0618
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        var page = GetCurrentPage();
        if (page is not null)
        {
#pragma warning disable CS0618 // DisplayAlert is obsolete but DisplayAlertAsync is not available in all MAUI versions
            return await page.DisplayAlert(title, message, accept, cancel);
#pragma warning restore CS0618
        }
        return false;
    }

    private static Page? GetCurrentPage()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        return window?.Page;
    }
}

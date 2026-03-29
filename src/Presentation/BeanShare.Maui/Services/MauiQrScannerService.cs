using BeanShare.SharedUi.Services;

namespace BeanShare.Maui.Services;

public sealed class MauiQrScannerService : IQrScannerService
{
    public bool IsSupported => true;

    public async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.CheckStatusAsync<Permissions.Camera>());

        if (status == PermissionStatus.Granted)
            return true;

        status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.RequestAsync<Permissions.Camera>());

        return status == PermissionStatus.Granted;
    }
}

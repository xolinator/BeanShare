namespace BeanShare.SharedUi.Services;

public interface IQrScannerService
{
    bool IsSupported { get; }
    Task<bool> RequestCameraPermissionAsync();
}

public sealed class BrowserQrScannerService : IQrScannerService
{
    public bool IsSupported => true;
    public Task<bool> RequestCameraPermissionAsync() => Task.FromResult(true);
}

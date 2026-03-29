using Android.Webkit;

namespace BeanShare.Maui.Platforms.Android;

public class CameraWebChromeClient : WebChromeClient
{
    private readonly WebChromeClient? _innerClient;

    public CameraWebChromeClient(WebChromeClient? innerClient = null)
    {
        _innerClient = innerClient;
    }

    public override void OnPermissionRequest(PermissionRequest? request)
    {
        if (request?.GetResources() != null)
        {
            request.Grant(request.GetResources());
        }
    }

    public override void OnPermissionRequestCanceled(PermissionRequest? request)
    {
        _innerClient?.OnPermissionRequestCanceled(request);
    }
}

using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.ScreenWake;
public class ScreenWakeLockService
{
    private readonly string className = "ScreenWakeLockService";
    private CommonLib commonLib = new CommonLib();
    private IJSRuntime? jsRuntime;

    public void AttachObject(IJSRuntime IJSRuntime)
    {
        jsRuntime = IJSRuntime;
    }

    public async Task RequestWakeLockAsync()
    {
        try
        {
            await jsRuntime!.InvokeVoidAsync("wakeLock.request");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: RequestWakeLockAsync err:[{ex.HResult}]{ex.Message}");
        }
    }

    public async Task ReleaseWakeLockAsync()
    {
        try
        {
            await jsRuntime!.InvokeVoidAsync("wakeLock.release");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: ReleaseWakeLockAsync err:[{ex.HResult}]{ex.Message}");
        }
    }
}
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.ScreenWake;
public class WorkerService
{
    private readonly string className = "WorkerService";
    private CommonLib commonLib = new CommonLib();
    private IJSRuntime? jsRuntime;

    public void AttachObject(IJSRuntime IJSRuntime)
    {
        jsRuntime = IJSRuntime;
    }

    public async Task<string> StartWorker()
    {
        try
        {
            return await jsRuntime!.InvokeAsync<string>("workerManager.startWorker");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: RequestWakeLockAsync err:[{ex.HResult}]{ex.Message}");
        }

        return string.Empty;
    }

    public async Task StopWorker(string WorkerNo)
    {
        try
        {
            if (WorkerNo != null && WorkerNo != string.Empty)
            {
                Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: StopWorker WorkerNo={WorkerNo}");
                await jsRuntime!.InvokeVoidAsync("workerManager.stopWorker", WorkerNo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: ReleaseWakeLockAsync err:[{ex.HResult}]{ex.Message}");
        }
    }
}
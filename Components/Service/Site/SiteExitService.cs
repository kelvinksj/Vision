using Microsoft.JSInterop;

namespace TaskManagerWeb.Components.Service.Site;
public class SiteExitService
{
    private IJSRuntime js;
    private TaskCompletionSource? taskCompletionSource;

    public event EventHandler? SPAClosed;

    public SiteExitService(IJSRuntime Js)
    {
        js = Js;
    }

    public async Task SetSpaExit()
    {
        if (taskCompletionSource == null)
        {
            taskCompletionSource = new TaskCompletionSource();
            DotNetObjectReference<SiteExitService>? objRef = DotNetObjectReference.Create(this);
            await js.InvokeVoidAsync("blazor_setExitCheck", objRef, true);
            taskCompletionSource.SetResult();
        }

        if (!taskCompletionSource.Task.IsCompleted)
            await taskCompletionSource.Task;
    }

    [JSInvokable]
    public Task SpaExit()
    {
        SPAClosed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
/* Important remarks
    // Only can use in Blazor project
    // This is to run continous background task that can be stop after switching the web page.

    // Need to put the 2 codes below in OnAfterRenderAsync of the program that
    // is using this PeriodicInvoker class.
    periodicInvoker = new PeriodicInvoker(InvokeAsyncWrapper, 2000);
    periodicInvoker.StartPeriodicInvoke(Thread1);

    // Need to put this in DisposeAsync of the program that is using this PeriodicInvoker class.
    if (periodicInvoker != null)
    {
        await periodicInvoker.DisposeAsync();
    }

    // Need to put this InvokeAsyncWrapper in the program that will use this PeriodicInvoker class.
    private async Task InvokeAsyncWrapper(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        try
        {
            if (Action == null) throw new ArgumentException(nameof(Action));

            await Action(CancellationToken);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            throw;
        }
    }

    // Sample threading
    private async Task Thread1(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                await InvokeAsync(async () =>
                {
                    await Task.Delay(0, CancellationToken);
                });
            }
        }
        catch (Exception ex)
        {
            if (ex.HResult == -2146232798)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Warning
                );
                throw new ArgumentException(ex.Message);
            }
            else
            {
                CommonLib.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }
    }
*/

namespace TaskManagerWeb.Components.Helper;
public class PeriodicInvoker : IAsyncDisposable
{
    private readonly string className = "PeriodicInvoker";
    private CommonLib commonLib = new CommonLib();
    private CancellationTokenSource? cancellationTokenSource;
    private TaskCompletionSource<bool>? taskCompletionSource;
    private readonly Func<Func<CancellationToken, Task>, CancellationToken, Task> invokeAsyncWrapper;
    private readonly int delayMilliseconds = 2000;

    public PeriodicInvoker(Func<Func<CancellationToken, Task>, CancellationToken, Task> InvokeAsyncWrapper, int DelayMilliseconds)
    {
        Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: PeriodicInvoker triggered");
        this.invokeAsyncWrapper = InvokeAsyncWrapper;
        this.delayMilliseconds = DelayMilliseconds;
    }

    private async Task PeriodicInvokeAsync(
        Func<CancellationToken, Task> Action,
        int DelayMilliseconds,
        CancellationToken CancellationToken)
    {
        Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: PeriodicInvokeAsync triggered");

        try
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                await invokeAsyncWrapper(Action, CancellationToken);
                await Task.Delay(delayMilliseconds, CancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: PeriodicInvokeAsync err:[{ex.HResult}]{ex.Message}");
        }
        finally
        {
            taskCompletionSource?.TrySetResult(true);
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: PeriodicInvokeAsync done");
        }
    }

    public void StartPeriodicInvoke(Func<CancellationToken, Task> Action)
    {
        Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: StartPeriodicInvoke triggered");

        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            taskCompletionSource = new TaskCompletionSource<bool>();
            _ = PeriodicInvokeAsync(Action, delayMilliseconds, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: StartPeriodicInvoke err:[{ex.HResult}]{ex.Message}");
        }
    }

    public async Task StopPeriodicInvokeAsync()
    {
        Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: StopPeriodicInvokeAsync triggered");

        try
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();

                if (taskCompletionSource?.Task != null)
                {
                    await taskCompletionSource.Task;
                }

                cancellationTokenSource.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: StopPeriodicInvokeAsync err:[{ex.HResult}]{ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: DisposeAsync triggered");

        try
        {
            await StopPeriodicInvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: DisposeAsync err:[{ex.HResult}]{ex.Message}");
        }

        GC.SuppressFinalize(this);
    }
}
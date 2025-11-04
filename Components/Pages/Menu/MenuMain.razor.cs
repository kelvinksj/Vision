using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Pages.Menu;
public partial class MenuMain
{
    private string taskName = "MenuMain";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_MenuMain;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private string workerNo = string.Empty;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private int lineNo = 0;
    private bool isOperator = false;
    private bool isSupervisor = false;
    private string accessPage = string.Empty;

    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

        UpdateInfoChange();

        // Subscribe to URL customize LocationChanged event
        NavigationManager.LocationChanged += OnLocationChanged;

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitializedAsync triggered",
            settingLevel,
            MessageLevel.Information
        );
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        authUser = authState.User;
        currentUserName = authUser.Identity!.Name;

        if (!authUser.IsInRole("Administrator")
        && !authUser.IsInRole("Engineer")
        && !authUser.IsInRole("Supervisor")
        && !authUser.IsInRole("Operator"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        if (authUser.IsInRole("Operator"))
        {
            isOperator = true;
            var groupSidClaim = authUser.FindFirst(System.Security.Claims.ClaimTypes.GroupSid);
            accessPage = groupSidClaim?.Value.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(accessPage)
                && accessPage[..1] == "L"
            )
            {
                if (int.TryParse(accessPage[1..], out int result))
                    lineNo = result;
                else
                    lineNo = 0;
            }
        }

        if (authUser.IsInRole("Supervisor"))
        {
            isSupervisor = true;
        }

        if (!authUser.IsInRole("Administrator")
        && !authUser.IsInRole("Engineer")
        && !authUser.IsInRole("Supervisor")
        && !authUser.IsInRole("Operator"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        // CommonLib.DisplayConsole(
        //     taskName,
        //     $"OnAfterRenderAsync triggered",
        //     settingLevel,
        //     MessageLevel.Information
        // );

        if (FirstRender && urlPage == taskName)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnAfterRenderAsync first rendor triggered",
                settingLevel,
                MessageLevel.Information
            );

            try
            {
                registration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);

                workerNo = await WorkerService.StartWorker();
                await ScreenWakeLockService.RequestWakeLockAsync();
            }
            catch (Exception ex)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"OnAfterRenderAsync err:[{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }

        await base.OnAfterRenderAsync(FirstRender);
    }

    public void Dispose()
    // Need to add "@implements IDisposable" at the top of the razor file
    // and add ": IDisposable" after partial class on the same line
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Dispose triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"Dispose err:[{ex.HResult}]{ex.Message}",
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    protected async virtual void DisposeAsync(bool Disposing)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"DisposeAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (!disposed)
        {
            if (Disposing)
            // Cleanup managed resources
            {
                try
                {
                    // Unsubscribe customize LocationChanged event from URL
                    NavigationManager.LocationChanged -= OnLocationChanged;
                    registration?.Dispose();

                    await WorkerService.StopWorker(workerNo);
                    await ScreenWakeLockService.ReleaseWakeLockAsync();
                }
                catch (Exception ex)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"DisposeAsync err:[{ex.HResult}]{ex.Message}",
                        settingLevel,
                        MessageLevel.Error
                    );
                }
            }

            // Cleanup unmanaged resources (if any)
            disposed = true;
        }
    }

    // Finalizer (destructor)
    ~MenuMain()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    private ValueTask OnLocationChanging(LocationChangingContext context)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnLocationChanging triggered",
            settingLevel,
            MessageLevel.Information
        );
        // context.PreventNavigation();
        return ValueTask.CompletedTask;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    // Handle the URL change here (e.g., update styles, perform actions, etc.)
    {
        await UpdateInfoChange();
        await InvokeAsync(StateHasChanged);
    }

    private Task UpdateInfoChange()
    // Update all the necessary variable or task for the page when URL change
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateInfoChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            currentUrl = NavigationManager.Uri;
            urlPage = CommonLib.GetPageName(currentUrl, 0);

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange: urlPage={urlPage}, taskName={taskName}",
                settingLevel,
                MessageLevel.Trace
            );

            if (urlPage != taskName)
                return Task.CompletedTask;

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange done",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange err:[{ex.HResult}]{ex.Message}",
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private void Navigate1()
    {
        if (isOperator || isSupervisor)
            NavigationManager.NavigateTo("Account/Users", true, true);
        else
            NavigationManager.NavigateTo("Menu/MenuUser", true, true);
    }

    private void Navigate2()
    {
        NavigationManager.NavigateTo("Mattel/LayoutList", true, true);
    }

    private void Navigate3()
    {
        NavigationManager.NavigateTo("Menu/MenuKit", true, true);
    }

    private void Navigate4()
    {
        NavigationManager.NavigateTo("Mattel/OrderKitList", true, true);
    }

    private void Navigate5()
    {
        NavigationManager.NavigateTo("Mattel/LineList", true, true);
    }

    private void Navigate6()
    {
        if (isOperator)
        {
            if (accessPage == "CI")
                NavigationManager.NavigateTo("Mattel/CheckInPage", true, true);
            else if (accessPage == "CO")
                NavigationManager.NavigateTo("Mattel/CheckOutPage", true, true);
            else if (accessPage == "CPC")
                NavigationManager.NavigateTo("Mattel/CPCNew", true, true);
            else if (accessPage == "GL")
                NavigationManager.NavigateTo("Mattel/GaylordPage", true, true);
            else
                NavigationManager.NavigateTo("Mattel/LineRun", true, true);
        }
        else
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
    }
}
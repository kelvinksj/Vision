using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Setting;
using TaskManagerWeb.Components.Service.Setting;

namespace TaskManagerWeb.Components.Pages.Setting;
public partial class Settings : IDisposable
{
    private string taskName = "Settings";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_Settings;
    static string currentUrl = string.Empty;
    static string? urlPage = string.Empty;
    private bool disposed = false;
    ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private bool isNavigating = false;
    private SettingsInfo info = new SettingsInfo();
    private IDisposable? registration;

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
        )
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        // if (FirstRender && urlPage == taskName)
        if (FirstRender)
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
            }
            catch (Exception ex)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    ex,
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
                ex,
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
                }
                catch (Exception ex)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        ex,
                        settingLevel,
                        MessageLevel.Error
                    );
                }
            }

            // Cleanup unmanaged resources (if any)
            disposed = true;
        }

        await Task.CompletedTask;
    }

    // Finalizer (destructor)
    ~Settings()
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

            info = SettingsService.GetInfo();

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
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private async void SetDefault(SettingsInfo Info)
    {
        CommonLib.DisplayConsole(
             taskName,
             $"SetDefault triggered",
             settingLevel,
             MessageLevel.Information
        );

        try
        {
            Info.IsBusy = true;
            Info.IsDefault = true;
            info = SettingsService.GetDefault();
            Info.IsBusy = false;
            Info.IsDefault = false;
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void OnValidSubmit(SettingsInfo Info)
    {
        CommonLib.DisplayConsole(
             taskName,
             $"OnValidSubmit triggered",
             settingLevel,
             MessageLevel.Information
        );

        try
        {
            if (!isNavigating)
            {
                Info.IsBusy = true;
                Info.IsChecking = true;
                string err = string.Empty;

                if (err != string.Empty)
                {
                    await jsRuntime.InvokeVoidAsync("alert", err);
                    Info.IsChecking = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                info.Container2NetData.IP = info.ContainerNetData.IP;
                SettingsService.UpdateInfo(info);
                Info.IsBusy = false;
                Info.IsChecking = false;
                await JsInteropHelper.FocusMyHeader(jsRuntime);
                NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
            }
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void EditCancel()
    {
        CommonLib.DisplayConsole(
             taskName,
             $"EditCancel triggered",
             settingLevel,
             MessageLevel.Information
        );

        try
        {
            isNavigating = true;
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void OnClickReset(int LineNumber)
    {
        CommonLib.DisplayConsole(
             taskName,
             $"EditCancel triggered",
             settingLevel,
             MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            await InvokeAsync(StateHasChanged);

            var productionList = MattelService.GetProductionList();
            var productionData = productionList.ProductionData.FirstOrDefault(
                n => n.LineName == $"Line {LineNumber}"
            );

            if (productionData != null
                && !string.IsNullOrWhiteSpace(productionData.OrderKitNumber)
            )
            {
                productionData.Status = "Open";
                MattelService.UpdateProductionList(productionData);
                MattelService.StatusOrderKit(
                    productionData.OrderKitNumber,
                    productionData.Status
                );
            }
        }
        catch (Exception ex)
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
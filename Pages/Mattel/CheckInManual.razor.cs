using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CheckInManual : IDisposable
{
    [Parameter]
    public EventCallback<bool> ShowModalChanged { get; set; }

    [Parameter]
    public EventCallback<MattelClass.CheckInData> OnCheckIn { get; set; }

    // [Parameter]
    // public List<string>? Models { get; set; }

    [Parameter]
    public List<string>? DropdownOptions { get; set; }

    public string taskName = "CheckInManual";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_CheckInManual;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private MattelClass.CheckInData checkInData = new();
    // private string model = string.Empty;
    private string selectedPoint = string.Empty;
    private bool IsCheckInDisabled =>
        (
            string.IsNullOrWhiteSpace(checkInData.KitNumber)
            || string.IsNullOrWhiteSpace(checkInData.PartNumber)
            || string.IsNullOrWhiteSpace(selectedPoint)
        );
    private string disableColor =>
        IsCheckInDisabled
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

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
            && !authUser.IsInRole("Operator")
        )
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnAfterRenderAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (firstRender && urlPage == taskName)
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

        await base.OnAfterRenderAsync(firstRender);
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

    protected async virtual void DisposeAsync(bool disposing)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"DisposeAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (!disposed)
        {
            if (disposing)
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
    ~CheckInManual()
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

            // if (urlPage != taskName)
            //     return Task.CompletedTask;

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

    private async void CheckInputKit(ChangeEventArgs e)
    {
        checkInData.KitNumber = e.Value?.ToString() ?? string.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckInputPart(ChangeEventArgs e)
    {
        checkInData.PartNumber = e.Value?.ToString() ?? string.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckInputLoc(ChangeEventArgs e)
    {
        selectedPoint = e.Value?.ToString() ?? string.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseModal()
    {
        await ShowModalChanged.InvokeAsync(false); // Close the modal
    }

    private async Task Confirm()
    {
        checkInData.KitNumber = checkInData.KitNumber.Replace(" ", string.Empty);
        checkInData.PartNumber = checkInData.PartNumber.Replace(" ", string.Empty);
        checkInData.Point = selectedPoint.Replace(" ", string.Empty);
        await OnCheckIn.InvokeAsync(checkInData);
    }
}
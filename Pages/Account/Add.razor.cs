using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Authentication;
// using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Setting;

namespace TaskManagerWeb.Components.Pages.Account;
public partial class Add : IDisposable
{
    [Parameter]
    public int userID { get; set; } = 0;

    [Parameter]
    public EventCallback<bool> ShowModalChanged { get; set; }

    [Parameter]
    public EventCallback<UserAccount> OnUserAdded { get; set; }

    private string taskName = "Add";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_AccAdd;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private string? currentUserRole = string.Empty;
    private UserAccount user = new UserAccount();
    private UserAccount currentUser = new UserAccount();
    private IDisposable? registration;
    private string workerNo = string.Empty;
    private MattelClass.PageList page = new();
    private bool IsPageDisabled =>
        (
            !string.Equals(
                user.UserRole,
                nameof(Role.Operator),
                StringComparison.OrdinalIgnoreCase
            )
        );

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

        if (!authUser.IsInRole("Administrator"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        IList<Claim> authClaims = authUser.Claims.ToList();

        if (authUser != null && userID != 0)
        {
            currentUser = UserAccountService.GetByUserId(userID)!;
            currentUserRole = authClaims.FirstOrDefault(c => c.Type == ClaimTypes.Role)!.Value;
            // Load the page types from the service 
            page = MattelService.GetPageList();
        }

        CommonLib.DisplayConsole(
            taskName,
            $"OnInitializedAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
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

                    await WorkerService.StopWorker(workerNo);
                    await ScreenWakeLockService.ReleaseWakeLockAsync();
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
    }

    // Finalizer (destructor)
    ~Add()
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
            urlPage = CommonLib.GetPageName(currentUrl, 1);

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
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private async void CheckUsername(ChangeEventArgs e)
    {
        user.UserName = (e.Value?.ToString() ?? string.Empty).Replace(" ", string.Empty);
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckPassword(ChangeEventArgs e)
    {
        user.Password = (e.Value?.ToString() ?? string.Empty).Replace(" ", string.Empty);
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckFirstName(ChangeEventArgs e)
    {
        user.FirstName = (e.Value?.ToString() ?? string.Empty).Replace(" ", string.Empty);
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckLastName(ChangeEventArgs e)
    {
        if (e.Value != null)
        {
            // Replace multiple spaces with a single space
            user.LastName = System.Text.RegularExpressions.Regex.Replace(e.Value.ToString()!, @"\s+", " ").Trim();
        }
        else
        {
            user.LastName = string.Empty;
        }
        await InvokeAsync(StateHasChanged);
    }

    private void OnUserRoleChange()
    {
        // If the selected role is not "Operator," set Page to "Disable"
        if (!string.Equals(
                user.UserRole,
                nameof(Role.Operator),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            user.Page = "Disable";
        }

        // Refresh the component state
        StateHasChanged();
    }

    private async void OnValidSubmit(UserAccount User)
    {
        // Invoke the event callback to notify that the user has been added
        await OnUserAdded.InvokeAsync(user);
    }

    private async Task CloseModal()
    {
        await ShowModalChanged.InvokeAsync(false); // Close the modal
    }
}
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Service.Authentication;

namespace TaskManagerWeb.Components.Pages.Account;
public partial class Login
{
    [Parameter]
    public bool showModal { get; set; }

    [Parameter]
    public EventCallback<bool> ShowModalChanged { get; set; }

    private string taskName = "Login";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_AccLogin;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private Model model = new();
    private string errorMessage = string.Empty;
    private string modalContent = string.Empty;
    private string passwordInputType = "password";
    private string eyeIconClass = "icon_visibility";

    private bool IsLoginDisabled =>
        (
            string.IsNullOrWhiteSpace(model.UserName)
            || string.IsNullOrWhiteSpace(model.Password)
        );
    private string disableColor =>
        (
            string.IsNullOrWhiteSpace(model.UserName)
            || string.IsNullOrWhiteSpace(model.Password)
        )
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private class Model
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

        UpdateInfoChange();

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
    ~Login()
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
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private async Task CheckInput(ChangeEventArgs e)
    {
        model.UserName = e.Value?.ToString() ?? string.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CheckInput2(ChangeEventArgs e)
    {
        model.Password = e.Value?.ToString() ?? string.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private void TogglePasswordVisibility()
    {
        if (passwordInputType == "password")
        {
            passwordInputType = "text";
            eyeIconClass = "icon_visibility_off";
        }
        else
        {
            passwordInputType = "password";
            eyeIconClass = "icon_visibility";
        }
    }

    private async Task Authenticate()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Authenticate triggered",
            settingLevel,
            MessageLevel.Information
        );
        UserAccount? userAccount = UserAccountService.GetByUserName(model.UserName);

        if (userAccount == null || userAccount.Password != model.Password)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"Authenticate invalid user!",
                settingLevel,
                MessageLevel.Information
            );

            errorMessage = "User name or password invalid";
            await InvokeAsync(StateHasChanged);
            return;
        }

        CustomAuthenticationStateProvider? customAuthenticationStateProvider
        = (CustomAuthenticationStateProvider)AuthenticationStateProvider;
        await customAuthenticationStateProvider.UpdateAuthenticationState(new UserSession
        {
            UserId = userAccount.UserId.ToString(),
            UserName = userAccount.UserName,
            FirstName = userAccount.FirstName,
            LastName = userAccount.LastName,
            UserRole = userAccount.UserRole,
            Page = userAccount.Page
        });

        CommonLib.DisplayConsole(
            taskName,
            $"Authenticate UserRole={userAccount.UserName}",
            settingLevel,
            MessageLevel.Information
        );

        CommonLib.DisplayConsole(
            taskName,
            $"Authenticate UserRole={userAccount.UserRole}",
            settingLevel,
            MessageLevel.Information
        );

        CommonLib.DisplayConsole(
            taskName,
            $"Authenticate user validated",
            settingLevel,
            MessageLevel.Information
        );

        NavigationManager.NavigateTo("Menu/MenuMain", true, true);
    }

    private async Task ClosePopupModal()
    {
        await InvokeAsync(StateHasChanged);
        modalContent = "signin";
        showModal = true;
    }

    private async Task CloseModal()
    {
        model.UserName = string.Empty;
        model.Password = string.Empty;
        errorMessage = string.Empty;
        eyeIconClass = "icon_visibility";
        passwordInputType = "password";
        await ShowModalChanged.InvokeAsync(false);
    }
}
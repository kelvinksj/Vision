using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Service.KeepAlive;

namespace TaskManagerWeb.Components.Layout;
public partial class MainLayout : IDisposable
{
    private readonly string softwareVersion = "v1.6.11";
    private string taskName = "MainLayout";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LayoutMain;
    private bool disposed = false;
    private IDisposable? registration;
    private string modalContent = string.Empty;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private string? currentUserRole = string.Empty;
    private int lineNo = 0;
    private bool isOperator = false;
    private bool isSupervisor = false;
    private string accessPage = string.Empty;
    public bool showModal { get; set; } = false;
    private Guid pageGuid = Guid.Empty;

    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

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

        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            authUser = authState.User;
            currentUserName = authUser.Identity?.Name ?? string.Empty;
            currentUserRole = authUser.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
            pageGuid = KeepAliveService.PageOpened(HttpClientFactory, NavigationManager, currentUserName);

            CommonLib.DisplayConsole(
                taskName,
                $"OnInitializedAsync: pageGuid={pageGuid}",
                settingLevel,
                MessageLevel.Trace
            );

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

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
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
                NavigationManager.LocationChanged += OnLocationChanged;
                registration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);

                //DotNetObjectReference<MainLayout>? dotNetObjectRef = DotNetObjectReference.Create(this);
                //await jsRuntime.InvokeVoidAsync("connectionStatus.initialize", dotNetObjectRef);
                ScreenWakeLockService.AttachObject(jsRuntime);
                WorkerService.AttachObject(jsRuntime);
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
                    await KeepAliveService.PageClosed(string.Empty);
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
    ~MainLayout()
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
        //Context.PreventNavigation();
        return ValueTask.CompletedTask;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    // Handle the URL change here (e.g., update styles, perform actions, etc.)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"MainLayout OnLocationChanged triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (e.IsNavigationIntercepted)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"MainLayout Reconnected to the server",
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    [JSInvokable]
    public async void UpdateConnectionStatus(bool isOnline)
    {
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public static DotNetObjectReference<MainLayout> GetDotNetObjectReference()
    {
        return DotNetObjectReference.Create(new MainLayout());
    }

    private async void ShowModalChanged(bool value)
    {
        showModal = value;
        await InvokeAsync(StateHasChanged);
    }

    private void ShowSigninModal()
    {
        showModal = true;
        modalContent = "signin";
    }

    private void GoBack()
    {
        if (NavigationManager.Uri.Contains(
            "/Mattel/SetupLayout",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/LayoutList", true, true);

        else if (NavigationManager.Uri.Contains(
            "/Mattel/SetupOrderKit",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/OrderKitList", true, true);


        else if (NavigationManager.Uri.Contains(
            "/Mattel/Manual/SetupKit",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuKit", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/Manual/AddKit",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/Manual/SetupKit", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/Manual/EditKit",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/Manual/SetupKit", true, true);

        else if (NavigationManager.Uri.Contains(
            "/Mattel/Scan/ScanOne",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuKit", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/Scan/ScanAll",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuKit", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/Scan/Manage",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/Scan/ScanAll", true, true);


        else if (NavigationManager.Uri.Contains(
            "/Mattel/CheckInPage",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CheckInPage",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CPCPage",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CPCPage",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CPCNew",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CPCNew",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/GaylordPage",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/GaylordPage",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/LineRun",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/LineRun",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CheckOutPage",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator)
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CheckOutPage",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/VAS",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Mattel/CheckOutPage", true, true);

        else if (NavigationManager.Uri.Contains(
            "/Account/Users",
            StringComparison.OrdinalIgnoreCase
        ) && !isOperator && !isSupervisor)
            NavigationManager.NavigateTo("Menu/MenuUser", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Menu/MenuMaintenance",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuUser", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Setting/Settings",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/MoveRobot",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/MoveTable",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/CheckInMaintenance",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);
        else if (NavigationManager.Uri.Contains(
            "/Mattel/DragDrop",
            StringComparison.OrdinalIgnoreCase
        ))
            NavigationManager.NavigateTo("Menu/MenuMaintenance", true, true);

        else
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
    }

    private bool IsHomePage()
    {
        bool reply
            = NavigationManager.Uri.Equals(
                NavigationManager.BaseUri,
                StringComparison.OrdinalIgnoreCase
            )
            || NavigationManager.Uri.Equals(
                $"{NavigationManager.BaseUri}Menu/MenuMain",
                StringComparison.OrdinalIgnoreCase
            );

        return reply;
    }

    private void Navigate0()
    {
        if (!IsHomePage())
            NavigationManager.NavigateTo("Menu/MenuMain", true, true);
    }

    private void Navigate1()
    {
        NavigationManager.NavigateTo("Account/Logout", true, true);
    }

    private void Navigate2()
    {
        if (isOperator || isSupervisor)
            NavigationManager.NavigateTo("Account/Users", true, true);
        else
            NavigationManager.NavigateTo("Menu/MenuUser", true, true);
    }

    private void Navigate3()
    {
        NavigationManager.NavigateTo("Mattel/LayoutList", true, true);
    }

    private void Navigate4()
    {
        NavigationManager.NavigateTo("Menu/MenuKit", true, true);
    }

    private void Navigate5()
    {
        NavigationManager.NavigateTo("Mattel/OrderKitList", true, true);
    }

    private void Navigate6()
    {
        NavigationManager.NavigateTo("Mattel/LineList", true, true);
    }

    private void Navigate7()
    {
        if (isOperator)
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
        else
            NavigationManager.NavigateTo("Menu/MenuLine", true, true);
    }
}
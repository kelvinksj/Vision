using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class TableEdit : IDisposable
{
    [Parameter]
    public PopTableEditData popTableEditData { get; set; } = new();

    private string taskName = "TableEdit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_TableEdit;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private UserAccount user = new UserAccount();
    private UserAccount currentUser = new UserAccount();
    private IDisposable? registration;
    private List<string> tableState = [];
    private List<string> tableType = [];

    private bool disableStatus =>
    (
        popTableEditData.Table.Type == Element.TableType.NA
    );

    private bool disableKit =>
    (
        popTableEditData.Table.Type == Element.TableType.NA
        || popTableEditData.Table.Type == Element.TableType.FG
        || popTableEditData.Table.Type == Element.TableType.Gaylord
    );

    private bool disablePart =>
    (
        popTableEditData.Table.Type == Element.TableType.NA
        || popTableEditData.Table.Type == Element.TableType.FG
        || popTableEditData.Table.Type == Element.TableType.Gaylord
    );

    private bool disableConfirm =>
    (
        (
            (popTableEditData.Table.Type == Element.TableType.NA
                || popTableEditData.Table.Type == Element.TableType.FG
                || popTableEditData.Table.Type == Element.TableType.Gaylord
            )
            && (!string.IsNullOrWhiteSpace(popTableEditData.Table.KitNumber)
                || !string.IsNullOrWhiteSpace(popTableEditData.Table.PartNumber)
            )
        )
        || (popTableEditData.Table.Type == Element.TableType.SKU
            && (string.IsNullOrWhiteSpace(popTableEditData.Table.KitNumber)
               || string.IsNullOrWhiteSpace(popTableEditData.Table.PartNumber)
            )
        )
    );
    private string disableStyleConfirm =>
        (
            disableConfirm
        )
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
        )
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

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
    ~TableEdit()
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

    public override async Task SetParametersAsync(ParameterView Parameters)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SetParametersAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {

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

        await base.SetParametersAsync(Parameters);
    }

    protected override async Task OnParametersSetAsync()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnParametersSetAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            tableType = [];

            foreach (var item in Element.TableType.All)
            {
                tableType.Add(item);
            }

            switch (popTableEditData.Table.Type)
            {
                case Element.TableType.FG:
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty,
                        Element.TableState.Loaded
                    ];
                    break;

                case Element.TableType.Gaylord:
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty,
                        Element.TableState.Full
                    ];
                    break;

                case Element.TableType.SKU:
                    popTableEditData.Table.Status = Element.TableState.Loaded;
                    tableState = [
                        Element.TableState.Loaded,
                        Element.TableState.Quarantine
                    ];
                    break;

                case Element.TableType.NA:
                default:
                    popTableEditData.Table.Type = Element.TableType.NA;
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty
                    ];
                    break;
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

        await Task.CompletedTask;
    }

    private void OnValidSubmit()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnValidSubmit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {

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

    private async Task Confirm()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"Confirm triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            if (popTableEditData?.EventOk.HasDelegate ?? false)
                await popTableEditData.EventOk.InvokeAsync();
            else
                throw new ArgumentException("EventOk has no delegate");
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

    private async Task Cancel()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"Cancel triggered",
           settingLevel,
           MessageLevel.Information
       );

        try
        {
            if (popTableEditData?.EventNok.HasDelegate ?? false)
                await popTableEditData.EventNok.InvokeAsync();
            else
                throw new ArgumentException("EventNok has no delegate");
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

    private async void HandleTypeChange()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"HandleTypeChange triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            switch (popTableEditData.Table.Type)
            {
                case Element.TableType.FG:
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty,
                    Element.TableState.Loaded
                    ];
                    break;

                case Element.TableType.Gaylord:
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty,
                    Element.TableState.Full
                    ];
                    break;

                case Element.TableType.SKU:
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    tableState = [
                        Element.TableState.Loaded,
                    Element.TableState.Quarantine
                    ];
                    break;

                case Element.TableType.NA:
                default:
                    popTableEditData.Table.Type = Element.TableType.NA;
                    popTableEditData.Table.Status = Element.TableState.Empty;
                    popTableEditData.Table.KitNumber = string.Empty;
                    popTableEditData.Table.PartNumber = string.Empty;
                    tableState = [
                        Element.TableState.Empty
                    ];
                    break;
            }

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

    private async void HandleKitChange(ChangeEventArgs e)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"HandleKitChange triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popTableEditData.Table.KitNumber = e.Value?.ToString()?.Replace(" ", "") ?? string.Empty;
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

    private async void HandlePartChange(ChangeEventArgs e)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"HandlePartChange triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popTableEditData.Table.PartNumber = e.Value?.ToString()?.Replace(" ", "") ?? string.Empty;
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
}
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class LayoutList : IDisposable
{
    private string taskName = "LayoutList";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LayoutList;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;

    private bool disableBtn =>
        (
            selectedData == null
        );
    private string disableStyle =>
        (
            selectedData == null
        )
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private MattelClass.LayoutList layoutList = new();
    private MattelClass.LayoutData selectedData = null!;
    private PopMsgData popMsgData = new();

    public Dictionary<string, string> SortIcons { get; set; } = new Dictionary<string, string>
    {
        {"LayoutName", "icon_signin"},
        {"SetupType", "icon_signin"},
        {"SKU", "icon_signin"},
        {"GL", "icon_signin"},
        {"FG", "icon_signin"}
    };

    public Dictionary<string, bool> SortAscending { get; set; } = new Dictionary<string, bool>
    {
        {"LayoutName", true},
        {"SetupType", true},
        {"SKU", true},
        {"GL", true},
        {"FG", true}
    };

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
        )
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

        await Task.CompletedTask;
    }

    // Finalizer (destructor)
    ~LayoutList()
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

            var tempList = MattelService.GetLayoutList();
            layoutList.List = tempList.List.OrderBy(
                n => n.LayoutName
            ).ToList();

            // setups = SetupService.GetSetupList().ToList();
            // filteredSetups = new List<Setup>(setups);
            PaginationService.ItemsPerPage = 8;
            PaginationService.SetItems(layoutList.List);

            var columns = new List<string> { "LayoutName", "LayoutTypeName", "countSKU", "countGL", "countFG" };
            SortingModel = new SortingModel<MattelClass.LayoutData>(columns);
            Sort();

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

    private void SelectRow(MattelClass.LayoutData LayoutData)
    {
        selectedData = LayoutData;
    }

    private string GetRow(MattelClass.LayoutData LayoutData)
    {
        return
            (
                selectedData != null
                && selectedData.ID == LayoutData.ID
            )
            ? "row-active"
            : string.Empty;
    }

    private async void OnPageChanged()
    {
        selectedData = null!;
        await InvokeAsync(StateHasChanged);
    }

    private async void Filter(string filter)
    {
        selectedData = null!;
        var filterList = layoutList.List;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterList = layoutList.List.Where(u =>
                u.LayoutName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.LayoutTypeName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.countSKU.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.countGL.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.countFG.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        PaginationService.SetItems(filterList);
        PaginationService.CurrentPage = 1;
        await InvokeAsync(StateHasChanged);
    }

    private async void Sort()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Sort triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Set initial sorting for the first column (UserName) 
            SortingModel.SortStates["LayoutName"] = SortState.Descending;
            SortingModel.SortIcons["LayoutName"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var layoutData = await SortingModel.SortItemsAsync(layoutList.List, "LayoutName");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(layoutData);
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

    private async Task SortItems(string column)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SortItems triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var layoutData = await SortingModel.SortItemsAsync(layoutList.List, column);
            PaginationService.SetItems(layoutData); // Reset pagination
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

    public void OnClickAdd()
    {
        NavigationManager.NavigateTo($"/Mattel/SetupLayout/-1", true, true);
    }

    public void OnClickEdit()
    {
        if (selectedData != null)
        {
            NavigationManager.NavigateTo($"/Mattel/SetupLayout/{selectedData.ID}", true, true);
        }
    }

    public async void OnClickDelete()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickDelete triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                if (MattelService.Layout.IsLayoutUsed(selectedData.LayoutName))
                {
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"The layout \"{selectedData.LayoutName}\" is still being";
                    popMsgData.Text_2 = $"use and cannot be deleted";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickCancel();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                }
                else
                {
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = "Are you sure you want";
                    popMsgData.Text_2 = $"to remove \"{selectedData.LayoutName}\" ?";
                    popMsgData.Middle_Icon = "icon_remove";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
                    popMsgData.BtnText_2 = "Cancel";
                    popMsgData.BtnClick_2 = () => OnClickCancel();
                    await InvokeAsync(StateHasChanged);

                    CommonLib.DisplayConsole(
                        taskName,
                        $"OnClickDelete data deleted",
                        settingLevel,
                        MessageLevel.Information
                    );
                }
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

    private void OnClickOk()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOk triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            layoutList.List.Remove(selectedData);
            MattelService.RemoveLayoutData(selectedData);
            PaginationService.SetItems(layoutList.List);
            selectedData = null!;
            NavigationManager.NavigateTo("Mattel/LayoutList", true, true);
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

    private void OnClickCancel()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOk triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
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
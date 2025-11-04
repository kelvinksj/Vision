using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class OrderKitList : IDisposable
{
    private string taskName = "OrderKitList";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_OrderKitList;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private UserAccount user = new UserAccount();
    private IDisposable? registration;
    private string workerNo = string.Empty;
    private string? currentLayoutName = string.Empty;
    private ClaimsPrincipal authSetup = new ClaimsPrincipal();

    private bool disableEdit =>
        (
            selectedData == null
            || !string.IsNullOrEmpty(selectedData.Status)
        );
    private string styleEdit =>
        disableEdit
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableDuplicate =>
        (
            selectedData == null
        );
    private string styleDuplicate =>
        disableDuplicate
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableView =>
        (
            selectedData == null
        );
    private string styleView =>
        disableView
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableDelete =>
        (
            selectedData == null
            || (!string.IsNullOrEmpty(selectedData.Status)
                && !selectedData.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase)
            )
        );
    private string styleDelete =>
        disableDelete
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.OrderKit selectedData = null!;
    private RenderFragment? dynamicComponent;
    private int viewID = 0;
    private PopEditData popEditData { get; set; } = new();
    private PopMsgData popMsgData = new();

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
    ~OrderKitList()
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

            popEditData.SetEventCallbacks(this, ConfirmDuplicate, CancelDuplicate);

            var tempList = MattelService.GetOrderKitList();
            orderKitList.OrderKit = tempList.OrderKit.OrderBy(
                n => n.OrderKitNumber
            ).ToList();

            PaginationService.ItemsPerPage = 8;
            PaginationService.GoToPage(1);
            PaginationService.SetItems(orderKitList.OrderKit);
            selectedData = null!;

            var columns = new List<string> { "OrderKitNumber", "LayoutName", "Status", "CreatedData" };
            SortingModel = new SortingModel<MattelClass.OrderKit>(columns);
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
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private void SetDynamicComponent(
        string ComponentName,
        dynamic ParentVariable1,
        dynamic ParentVariable2
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SetDynamicComponent triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var componentType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(ComponentName, StringComparison.OrdinalIgnoreCase));

            if (componentType != null)
            {
                var callback
                    = EventCallback.Factory.Create<dynamic>(this, value => UpdateFromChild(value));

                dynamicComponent = builder =>
                {
                    builder.OpenComponent(0, componentType);
                    builder.AddAttribute(1, "ParentVariable1", ParentVariable1);
                    builder.AddAttribute(2, "ParentVariable2", ParentVariable2);
                    builder.AddAttribute(3, "ParentVariableChanged", callback);
                    builder.CloseComponent();
                };
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

    private Task UpdateFromChild(dynamic Data)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateFromChild triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            selectedData = null!;
            viewID = 0;
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

    private void SelectRow(MattelClass.OrderKit OrderKit)
    {
        selectedData = OrderKit;
    }

    private string GetRow(MattelClass.OrderKit OrderKit)
    {
        return
            (
                selectedData != null
                && selectedData.ID == OrderKit.ID
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
        var filterList = orderKitList.OrderKit;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterList = orderKitList.OrderKit.Where(u =>
                u.OrderKitNumber.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.LayoutName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.Status.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || u.CreatedData.ToString("MM-dd-yyyy").Contains(filter, StringComparison.OrdinalIgnoreCase)
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
            // Set initial sorting for the first column (OrderKitNumber) 
            SortingModel.SortStates["OrderKitNumber"] = SortState.Descending;
            SortingModel.SortIcons["OrderKitNumber"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var orders = await SortingModel.SortItemsAsync(orderKitList.OrderKit, "OrderKitNumber");

            // Ensure pagination service is updated with sorted
            PaginationService.SetItems(orders);
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
            var orders = await SortingModel.SortItemsAsync(orderKitList.OrderKit, column);
            PaginationService.SetItems(orders); // Reset pagination
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

    public void OnClickQuick()
    {
        NavigationManager.NavigateTo($"/Mattel/QuickSetup", true, true);
    }

    public void OnClickAdd()
    {
        NavigationManager.NavigateTo($"/Mattel/SetupOrderKit/-1", true, true);
    }

    private void OnClickEdit()
    {
        if (selectedData != null)
        {
            NavigationManager.NavigateTo($"/Mattel/SetupOrderKit/{selectedData.ID}", true, true);
        }
    }

    private async void OnClickDelete()
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
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Are you sure you want";
                popMsgData.Text_2 = $"to Remove \"{selectedData.KitNumber}\" ?";
                popMsgData.Middle_Icon = "icon_remove";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => ConfirmRemove();
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_2 = () => CloseRemoveModal();
                await InvokeAsync(StateHasChanged);

                CommonLib.DisplayConsole(
                    taskName,
                    $"OnClickDelete data deleted",
                    settingLevel,
                    MessageLevel.Information
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

    private async void ConfirmRemove()
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
                orderKitList.OrderKit.Remove(selectedData);
                MattelService.RemoveOrderKit(selectedData);

                var sheetList = MattelService.GetSheetList();
                var findKitNumber = sheetList.Sheet.FirstOrDefault(
                    n => n.KitNumber == selectedData.KitNumber
                );

                if (findKitNumber != null)
                    MattelService.RemoveKit(findKitNumber);

                var productionList = MattelService.GetProductionList();
                var findLine = productionList?.ProductionData.FirstOrDefault(
                    n => n.OrderKitNumber == selectedData.OrderKitNumber
                );

                if (findLine != null)
                {
                    findLine.OrderKitNumber = string.Empty;
                    findLine.Status = string.Empty;
                    MattelService.UpdateProductionList(findLine);
                }

                PaginationService.SetItems(orderKitList.OrderKit);
                selectedData = null!;
                CloseRemoveModal();
                await InvokeAsync(StateHasChanged);

                CommonLib.DisplayConsole(
                    taskName,
                    $"OnClickDelete data deleted",
                    settingLevel,
                    MessageLevel.Information
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

    private async void CloseRemoveModal()
    {
        selectedData = null!;
        await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnClickView()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickView triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                viewID = selectedData.ID;
                SetDynamicComponent(
                    "LineView",
                    viewID,
                    true
                );

                await JsInteropHelper.FocusMyHeader(jsRuntime);
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

    private void OnClickDuplicate()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickDuplicate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                popEditData.IsVisible = true;
                popEditData.Header = "Duplicate Kit Layout";
                popEditData.OldValue = selectedData.OrderKitNumber;
                popEditData.NewValue = string.Empty;
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

    private Task ConfirmDuplicate()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ConfirmDuplicate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (!string.IsNullOrWhiteSpace(popEditData.NewValue))
            {
                var newOrderKitNumber = popEditData.NewValue.Replace(" ", string.Empty);
                var isFound = MattelService.IsDuplicateOrderKitNumber(newOrderKitNumber);

                if (isFound)
                {
                    // Order kit number already exist. Cannot duplicate.
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Kit layout name \"{newOrderKitNumber}\"";
                    popMsgData.Text_2 = "already exist";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                }
                else
                {
                    // Can start to duplicate.
                    // Find old order kit number for duplication.
                    var findKitLayout = orderKitList.OrderKit.FirstOrDefault(
                        n => n.OrderKitNumber == popEditData.OldValue
                    );

                    if (findKitLayout != null)
                    {
                        // Found old order kit number. Can duplicate.
                        var sheetList = MattelService.GetSheetList();
                        // Find kit number already exist.
                        var findKitNumber = sheetList.Sheet.FirstOrDefault(
                            n => n.KitNumber == popEditData.NewValue
                        );

                        if (findKitNumber == null)
                        {
                            // Kit number not found. Can continue to duplicate.
                            // Find old kit number for duplication.
                            var oldKitNumber = sheetList.Sheet.FirstOrDefault(
                                n => n.KitNumber == popEditData.OldValue
                            );

                            if (oldKitNumber != null)
                            {
                                // Found old kit number data to be duplicate.
                                findKitLayout.OrderKitNumber = popEditData.NewValue;
                                findKitLayout.KitNumber = popEditData.NewValue;
                                findKitLayout.Status = string.Empty;
                                oldKitNumber.KitNumber = popEditData.NewValue;
                                MattelService.AddKit(oldKitNumber);
                                MattelService.AddOrderKit(findKitLayout);
                                UpdateInfoChange();

                                popMsgData.ActivatePopup = true;
                                popMsgData.Text_1 = $"Kit layout name \"{newOrderKitNumber}\"";
                                popMsgData.Text_2 = "added succesfully";
                                popMsgData.Middle_Icon = "icon_web_link";
                                popMsgData.BtnText_1 = "OK";
                                popMsgData.BtnClick_1 = () => OnClickOk();
                                popMsgData.BtnText_2 = null!;
                                popMsgData.BtnClick_2 = null!;
                                popEditData.IsVisible = false;
                                StateHasChanged();
                            }
                            else
                            {
                                // Old kit number not found. Database error.
                                popMsgData.ActivatePopup = true;
                                popMsgData.Text_1 = $"Old kit number \"{newOrderKitNumber}\"";
                                popMsgData.Text_2 = "not found in database";
                                popMsgData.Middle_Icon = "icon_issue";
                                popMsgData.BtnText_1 = "OK";
                                popMsgData.BtnClick_1 = () => OnClickOk();
                                popMsgData.BtnText_2 = null!;
                                popMsgData.BtnClick_2 = null!;
                            }
                        }
                        else
                        {
                            // Kit number already exist. Popup to confirm override data.
                            popMsgData.ActivatePopup = true;
                            popMsgData.Text_1 = $"Kit number \"{newOrderKitNumber}\"";
                            popMsgData.Text_2 = "already exist";
                            popMsgData.Middle_Icon = "icon_issue";
                            popMsgData.BtnText_1 = "Override";
                            popMsgData.BtnClick_1 = () => OnClickOverride();
                            popMsgData.BtnText_2 = "Cancel";
                            popMsgData.BtnClick_2 = () => CancelOverride();
                        }
                    }
                    else
                    {
                        // Old kit number not found. Database error.
                        popMsgData.ActivatePopup = true;
                        popMsgData.Text_1 = $"Old kit layout name \"{newOrderKitNumber}\"";
                        popMsgData.Text_2 = "not found in database";
                        popMsgData.Middle_Icon = "icon_issue";
                        popMsgData.BtnText_1 = "OK";
                        popMsgData.BtnClick_1 = () => OnClickOk();
                        popMsgData.BtnText_2 = null!;
                        popMsgData.BtnClick_2 = null!;
                    }
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

        return Task.CompletedTask;
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

    private void OnClickOverride()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOverride triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var newOrderKitNumber = popEditData.NewValue.Replace(" ", string.Empty);
            var findKitLayout = orderKitList.OrderKit.FirstOrDefault(
                n => n.OrderKitNumber == popEditData.OldValue
            );
            findKitLayout!.OrderKitNumber = newOrderKitNumber;
            findKitLayout!.KitNumber = newOrderKitNumber;
            var sheetList = MattelService.GetSheetList();
            var oldKitNumber = sheetList.Sheet.FirstOrDefault(
                n => n.KitNumber == popEditData.OldValue
            );
            var newKitNumber = sheetList.Sheet.FirstOrDefault(
                n => n.KitNumber == newOrderKitNumber
            );
            oldKitNumber!.KitNumber = newOrderKitNumber;
            oldKitNumber!.ID = newKitNumber!.ID;
            MattelService.EditKit(oldKitNumber);
            MattelService.AddOrderKit(findKitLayout);
            UpdateInfoChange();

            popEditData.IsVisible = false;
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = $"Kit layout name \"{newOrderKitNumber}\"";
            popMsgData.Text_2 = "added succesfully";
            popMsgData.Middle_Icon = "icon_web_link";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnClick_1 = () => OnClickOk();
            popMsgData.BtnText_2 = null!;
            popMsgData.BtnClick_2 = null!;
            popEditData.IsVisible = false;
            StateHasChanged();
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

    private void CancelOverride()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"CancelOverride triggered",
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

    private Task CancelDuplicate()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"CancelDuplicate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            popEditData.IsVisible = false;
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
}
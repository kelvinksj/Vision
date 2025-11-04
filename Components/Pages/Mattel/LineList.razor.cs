using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Reflection;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class LineList : IDisposable
{
    private string taskName = "LineList";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LineList;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private MattelClass.ProductionList productionList = new();
    private MattelClass.ProductionData selectedData = null!;
    private RenderFragment? dynamicComponent;

    private bool disableView =>
        (
            string.IsNullOrEmpty(selectedData?.OrderKitNumber)
        );
    private string bgColor_View =>
        (
            string.IsNullOrEmpty(selectedData?.OrderKitNumber)
        )
        ? "background-color: #a5a8a9;"
        : "background-color: #f36d33;";
    private string borderColor_View =>
        (
            string.IsNullOrEmpty(selectedData?.OrderKitNumber)
        )
        ? "border-color: #a5a8a9;"
        : "border-color: #f36d33;";

    private bool disableAssign =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        );
    private string bgColor_Assign =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        )
        ? "background-color: #a5a8a9;"
        : "background-color: #f36d33;";
    private string borderColor_Assign =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        )
        ? "border-color: #a5a8a9;"
        : "border-color: #f36d33;";

    private bool disableClear =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        );
    private string bgColor_Clear =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        )
        ? "background-color: #a5a8a9;"
        : "background-color: #f36d33;";
    private string borderColor_Clear =>
        (
            !string.IsNullOrEmpty(selectedData?.Status)
            && selectedData?.Status.Contains("Open") == false
            && selectedData?.Status.Contains("Completed") == false
        )
        ? "border-color: #a5a8a9;"
        : "border-color: #f36d33;";

    private int viewID = 0;

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
    ~LineList()
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

            productionList = MattelService.GetProductionList();
            PaginationService.ItemsPerPage = 6;
            PaginationService.SetItems(productionList.ProductionData);

            var columns = new List<string> { "LineName", "OrderKitNumber", "Status", "CreatedData" };
            SortingModel = new SortingModel<MattelClass.ProductionData>(columns);
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

    private async Task UpdateFromChild(dynamic Data)
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

            if (Data is MattelClass.ProductionData)
            {
                MattelClass.ProductionData passData = Data;
                var updateData = productionList.ProductionData.FirstOrDefault(
                    n => n.ID == passData.ID
                );

                if (updateData != null)
                {
                    MattelService.StatusOrderKit(updateData.OrderKitNumber, string.Empty);
                    updateData.OrderKitNumber = passData.OrderKitNumber;
                    updateData.Status = passData.Status;
                    MattelService.UpdateProductionList(passData);
                    MattelService.StatusOrderKit(passData.OrderKitNumber, passData.Status);
                    PaginationService.SetItems(productionList.ProductionData);
                    await InvokeAsync(StateHasChanged);

                    CommonLib.DisplayConsole(
                        taskName,
                        $"UpdateFromChild data updated",
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

        await Task.CompletedTask;
    }

    private void SelectRow(MattelClass.ProductionData ProductionData)
    {
        selectedData = ProductionData;
    }

    private string GetRow(MattelClass.ProductionData ProductionData)
    {
        return selectedData != null && selectedData.ID == ProductionData.ID ? "row-active" : string.Empty;
    }

    private async void OnPageChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void Filter(string filter)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Filter triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var filterList = productionList.ProductionData;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterList = productionList.ProductionData.Where(u =>
                    u.LineName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.OrderKitNumber.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.Status.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.CreatedData.ToString("MM-dd-yyyy").Contains(filter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            PaginationService.SetItems(filterList);
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
            SortingModel.SortStates["LineName"] = SortState.Descending;
            SortingModel.SortIcons["LineName"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var productionData = await SortingModel.SortItemsAsync(productionList.ProductionData, "LineName");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(productionData);
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
            var productionData = await SortingModel.SortItemsAsync(productionList.ProductionData, column);
            PaginationService.SetItems(productionData); // Reset pagination
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
                    false
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

    private async void OnClickAssign()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickAssign triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                viewID = selectedData.ID;
                SetDynamicComponent(
                    "LineEdit",
                    viewID,
                    false
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

    private async void OnClickClear()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickClear triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                if (selectedData.Status != "Completed")
                    MattelService.StatusOrderKit(selectedData.OrderKitNumber, string.Empty);

                selectedData.OrderKitNumber = string.Empty;
                selectedData.CreatedData = DateTime.Now;
                selectedData.Status = string.Empty;
                selectedData.Status = string.Empty;

                MattelService.UpdateProductionList(selectedData);
                PaginationService.SetItems(productionList.ProductionData);
                await InvokeAsync(StateHasChanged);
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
}
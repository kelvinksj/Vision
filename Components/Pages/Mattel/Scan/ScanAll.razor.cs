using System.Resources;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Page;

namespace TaskManagerWeb.Components.Pages.Mattel.Scan;
public partial class ScanAll : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private string taskName = "ScanAll";
    private MessageLevel settingLevel = DebugParameters.MsgLvl_ScanAll;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private MattelClass.SheetList sheets = new();
    private MattelClass.Sheet? selectedSheet = null!;
    private string? selectedKitNumber = null!;
    private string KitButtonColor
        => selectedKitNumber == null
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : "background-color: #f36d33; border-color: #f36d33;";
    private PopMsgData popMsgData = new();
    private bool isManage = false;
    private MsgList mList = new();
    private static bool isScanning = false;

    private bool disableScan =>
        (
            isScanning
        );
    private string disableStyleScan =>
        (
            disableScan
        )
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    public Dictionary<string, string> SortIcons { get; set; } = new Dictionary<string, string>
    {
        {"KitNumber", "icon_filter"},
        {"PartNumber", "icon_filter"}
    };

    public Dictionary<string, bool> SortAscending { get; set; } = new Dictionary<string, bool>
    {
        {"KitNumber", true},
        {"PartNumber", true}
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
        && !authUser.IsInRole("Supervisor"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnAfterRenderAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

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
                ScanService.SubscribeScan(LoadRawData, PageGuid);
                ScanService.SubscribeUI(MsgUpdate, PageGuid);
                ScanService.SubscribeActive(UpdateButtonActive, PageGuid);
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
                    ScanService.UnsubscribeScan(LoadRawData, PageGuid);
                    ScanService.UnsubscribeUI(MsgUpdate, PageGuid);
                    ScanService.UnsubscribeActive(UpdateButtonActive, PageGuid);

                    if (!isManage)
                        MattelService.SaveScanSheetList(new());
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
    ~ScanAll()
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

            sheets = MattelService.GetScanSheetList();

            if (sheets.IsStillUse)
            {
                sheets.IsStillUse = false;
                MattelService.SaveScanSheetList(sheets);
            }
            else
                sheets = new();

            KitPaginationService.ItemsPerPage = 8;
            KitPaginationService.SetItems(sheets.Sheet);
            PartPaginationService.ItemsPerPage = 8;

            var KitColumns = new List<string> { "KitNumber" };
            KitSortingModel = new SortingModel<MattelClass.Sheet>(KitColumns);

            var PartColumns = new List<string> { "PartNumber", "Description" };
            PartSortingModel = new SortingModel<MattelClass.SheetData>(PartColumns);

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

    public async void UpdateButtonActive(bool IsActive)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateButtonActive triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            isScanning = IsActive;
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

    public async void OnClickScan()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickScan triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await ScanService.RequestScan("all", "img", PageGuid);
            sheets = new();
            KitPaginationService.SetItems([]);
            PartPaginationService.SetItems([]);
            selectedSheet = null!;
            selectedKitNumber = null!;
            Sort();
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

    private async void LoadRawData(List<MattelClass.Sheet> ScanList)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"LoadRawData triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (ScanList?.Count > 0)
            {
                sheets = new();
                sheets.Sheet = ScanList.DeepClone();
                KitPaginationService.SetItems(sheets.Sheet);
                Sort();
                await InvokeAsync(StateHasChanged);
                MattelService.SaveScanSheetList(sheets);
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

    private void Manage()
    {
        if (selectedSheet != null)
        {
            isManage = true;
            sheets.IsStillUse = true;
            MattelService.SaveScanSheetList(sheets);
            NavigationManager.NavigateTo($"Mattel/Scan/Manage/{selectedSheet.ID}", true, true);
        }
    }

    private async void SelectKit(string kitNumber)
    {
        selectedKitNumber = kitNumber;
        selectedSheet = sheets.Sheet.FirstOrDefault(s => s.KitNumber == kitNumber);

        if (selectedSheet != null)
        {
            // Sort the part numbers in ascending order when a kit is selected 
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";
            // var sortedPart = PartSortingModel.SortItems(selectedSheet.SheetData, "PartNumber");
            PartPaginationService.GoToPage(1);
            // PartPaginationService.SetItems(sortedPart);
            PartPaginationService.SetItems(selectedSheet.SheetData);
        }

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
            //Sorting for Kit Table
            KitSortingModel.SortStates["KitNumber"] = SortState.Descending;
            KitSortingModel.SortIcons["KitNumber"] = "icon_sortAsc_darkGrey";
            var filteredKit = await KitSortingModel.SortItemsAsync(sheets.Sheet, "KitNumber");
            KitPaginationService.SetItems(filteredKit);

            //Sort for part table
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";
            PartSortingModel.SortStates["Description"] = SortState.Default;
            PartSortingModel.SortIcons["Description"] = "icon_filter_darkGrey";
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
            if (column == "KitNumber")
            {
                var filteredKit = await KitSortingModel.SortItemsAsync(sheets.Sheet, column);
                KitPaginationService.SetItems(filteredKit); // Reset pagination
            }
            else if (column == "PartNumber" && selectedSheet != null)
            {
                var sortedParts = await PartSortingModel.SortItemsAsync(selectedSheet.SheetData, column);
                PartPaginationService.SetItems(sortedParts); // Reset pagination
            }
            else if (column == "Description" && selectedSheet != null)
            {
                var sortedParts = await PartSortingModel.SortItemsAsync(selectedSheet.SheetData, column);
                PartPaginationService.SetItems(sortedParts); // Reset pagination
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

    private async void OnPageChanged()
    {
        selectedKitNumber = null!;
        PartPaginationService.SetItems(new List<MattelClass.SheetData>());
        PartPaginationService.GoToPage(1);
        await InvokeAsync(StateHasChanged);
    }

    private async void OnPageChanged2()
    {
        await Task.CompletedTask;
    }

    private async void MsgUpdate(List<HttpClass.UIMessage> UIMessages)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"MsgUpdate triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            if (UIMessages != null)
            {
                var count = 1;
                mList = new();

                foreach (var msg in UIMessages)
                {
                    DisplayString title = new();
                    CommonLib.CopyProperties(msg.Title, title);
                    List<DisplayString> textList = new();

                    foreach (var text in msg.TextList)
                    {
                        DisplayString text2 = new();
                        CommonLib.CopyProperties(text, text2);
                        textList.Add(text2);
                    }

                    mList.List.Add(
                        new()
                        {
                            ID = count++,
                            TaskID = msg.TaskID,
                            Title = title,
                            TextList = textList,
                            OnClick = () => MsgCancel(msg.TaskID)
                        }
                    );
                }

                await InvokeAsync(StateHasChanged);
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

    private void MsgCancel(string TaskID)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"MsgCancel triggered",
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
}
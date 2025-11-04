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
using TaskManagerWeb.Components.Service.DR;
using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Page;

namespace TaskManagerWeb.Components.Pages.Mattel.Scan;
public partial class ScanOne : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private string taskName = "ScanOne";
    private MessageLevel settingLevel = DebugParameters.MsgLvl_ScanOne;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;

    private MattelClass.SheetList sheets = new();
    private MattelClass.Sheet? selectedSheet;
    private MattelClass.SheetData? selectedPart;
    private MattelClass.SheetData? selectedRaw;
    private List<MattelClass.SheetData>? rawData = new();
    private List<MattelClass.SheetData>? partData = new();
    private List<string> partNumbers = new();
    private List<MattelClass.SheetData> originalData = new();

    private string? selectedKitNumber;
    private string? originalKitNumber;
    private string? selectedPartNumber;
    private bool showModal = false;
    private string modalContent = string.Empty;
    private static bool isScanning = false;

    private MattelClass.Sheet? dataKit;
    private MattelClass.SheetData? dataPart;

    private bool disableEditKit =>
        (
            string.IsNullOrWhiteSpace(selectedKitNumber)
        );
    private string EditKitBtnStyle =>
        disableEditKit
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableAddPart =>
        (
            string.IsNullOrWhiteSpace(selectedKitNumber)
            || selectedRaw == null
        );
    private string addPartBtnStyle =>
        disableAddPart
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableEditPart =>
        (
            selectedPart == null
        );
    private string editPartBtnStyle =>
        disableEditPart
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableRmvPart =>
        (
            selectedPart == null
        );
    private string rmvPartBtnStyle =>
        disableRmvPart
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableReset =>
        (
            string.IsNullOrWhiteSpace(selectedKitNumber)
            || partData?.Count == 0
        );
    private string resetBtnStyle =>
        disableReset
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableSave =>
        (
            string.IsNullOrWhiteSpace(selectedKitNumber)
            || partData?.Count == 0
        );
    private string saveBtnStyle =>
        disableSave
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableScan =>
        (
            isScanning
        );
    private string disableStyleScan =>
        disableScan
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private PopMsgData popMsgData = new();
    private MsgList mList = new();

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
                    ScanService.UnsubscribeScan(LoadRawData, PageGuid);
                    ScanService.UnsubscribeUI(MsgUpdate, PageGuid);
                    ScanService.UnsubscribeActive(UpdateButtonActive, PageGuid);
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
    ~ScanOne()
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

            PartPaginationService.ItemsPerPage = 8;
            PartPaginationService.SetItems(new List<MattelClass.SheetData>());

            SortRawItems();

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
            await ScanService.RequestScan("multi", "img", PageGuid);
            rawData = [];
            partData = [];
            originalData = [];
            originalKitNumber = string.Empty;
            RawPaginationService.SetItems([]);
            PartPaginationService.SetItems([]);
            selectedSheet = null!;
            selectedKitNumber = null!;
            SortRawItems();
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

        if (ScanList?.Count > 0)
        {
            rawData = [.. ScanList[0].SheetData];
            originalData = rawData.Select(data => new MattelClass.SheetData
            {
                ID = data.ID,
                PartNumber = data.PartNumber,
                Description = data.Description
            }).ToList(); // Store an immutable copy of the original data
            originalKitNumber = ScanList[0].KitNumber; // Store the original kit number
            RawPaginationService.ItemsPerPage = 8;
            RawPaginationService.SetItems(rawData);
            selectedSheet = ScanList.First();
            selectedKitNumber = selectedSheet.KitNumber;
            SortRawItems();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void SelectRaw(MattelClass.SheetData raw)
    {
        selectedRaw = raw;
        selectedPart = null;
        await InvokeAsync(StateHasChanged);
    }

    private void SelectPartNumber(MattelClass.SheetData part)
    {
        selectedPart = part;
        selectedPartNumber = part.PartNumber;
        selectedRaw = null;
    }

    private void EditKit()
    {
        if (selectedKitNumber != null)
        {
            dataKit = new MattelClass.Sheet
            {
                KitNumber = selectedKitNumber
            };
            modalContent = "EditKit";
            showModal = true;
        }
    }

    private void ShowEditKitModalChanged(bool show)
    {
        showModal = show;
    }

    private async Task OnKitEdited(MattelClass.Sheet editedKit)
    {
        if (selectedSheet != null)
        {
            // Update the selected sheet directly
            selectedSheet.KitNumber = editedKit.KitNumber;
            selectedSheet.Description = editedKit.Description;

            //Update the Kit Number in the input box
            selectedKitNumber = editedKit.KitNumber;

            // Optionally, update the parts if they were also edited in the kit
            // Only update parts if they have been edited in the Kit
            if (editedKit.SheetData != null && editedKit.SheetData.Count > 0)
            {
                selectedSheet.SheetData = editedKit.SheetData;
            }
        }
        showModal = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void AddPart()
    {
        if (!string.IsNullOrEmpty(selectedKitNumber) && selectedRaw != null)
        {
            partData ??= new List<MattelClass.SheetData>();

            // Check for duplicate in the second table 
            var existingPartData = partData.FirstOrDefault(p => p.PartNumber == selectedRaw.PartNumber);

            if (existingPartData != null)
            {
                // Part number already exists in the second table
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Part number already exists in the part table.";
                popMsgData.Text_2 = "";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseAddPartModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Logging selected raw data before operations

            // Add the selected raw data to the part table
            if (!partData.Contains(selectedRaw))
            {
                partData.Add(selectedRaw);
            }

            rawData ??= new List<MattelClass.SheetData>();

            // Find the index of the selected raw data 
            int selectedIndex = rawData.IndexOf(selectedRaw);

            // Remove the selected raw data from the raw table 
            if (rawData.Contains(selectedRaw))
            {
                rawData.Remove(selectedRaw);

                // Select the next item in the rawData list 
                if (selectedIndex >= 0 && selectedIndex < rawData.Count)
                {
                    selectedRaw = rawData[selectedIndex];
                }
                else
                {
                    selectedRaw = rawData.FirstOrDefault();
                }
            }

            // Update pagination services
            RawPaginationService.SetItems(rawData ?? new List<MattelClass.SheetData>());
            PartPaginationService.SetItems(partData ?? new List<MattelClass.SheetData>());

            // Reset sorting state and icon for the part table
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

            // Clear selectedRaw
            // selectedRaw = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CloseAddPartModal()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private void EditPart()
    {
        if (selectedPart != null)
        {
            dataPart = new MattelClass.SheetData
            {
                ID = selectedPart.ID,
                PartNumber = selectedPart.PartNumber,
                Description = selectedPart.Description
            };
            modalContent = "editPart";
            showModal = true;
        }
    }

    private void ShowEditPartModalChanged(bool show)
    {
        showModal = show;
    }

    private async void OnPartEdited(MattelClass.SheetData editedPart)
    {
        // Locate the original part in the list by ID
        var partToEdit = partData?.FirstOrDefault(p => p.ID == editedPart.ID);

        // Check if the new part number already exists but isn't the same as the current part
        var existingPart = partData?.FirstOrDefault(c => c.PartNumber == editedPart.PartNumber && c.ID != editedPart.ID);
        var existingRaw = rawData?.FirstOrDefault(c => c.PartNumber == editedPart.PartNumber);

        //if (selectedSheet?.SheetData != null)
        if (partToEdit != null)
        {
            if (existingPart != null || existingRaw != null)
            {
                // Duplicate detected
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "The part number already exists.";
                popMsgData.Text_2 = "Please use a different part number.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseEditPartModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }
            // Update part details if no duplicate exists
            partToEdit.PartNumber = editedPart.PartNumber;
            partToEdit.Description = editedPart.Description;

            // Update the part numbers list for display
            if (selectedSheet != null)
                partNumbers = selectedSheet.SheetData.Select(p => p.PartNumber).ToList();

            // Reset sorting state and icon for the part table
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

            // Reflect changes in the UI
            await InvokeAsync(StateHasChanged);

            // Close the modal after editing
            showModal = false;
        }
    }

    private async Task CloseEditPartModal()
    {
        popMsgData.ActivatePopup = false;
        modalContent = "editPart";
        showModal = true;
        await InvokeAsync(StateHasChanged);
    }

    private async void RmvPart()
    {
        if (selectedPart != null)
        {
            // Add the selected part data back to the raw table
            rawData?.Add(selectedPart);

            // Remove the selected part data from the part table
            // partData?.Remove(selectedPart);

            // Find the index of the selected part data
            int selectedIndex = partData?.IndexOf(selectedPart) ?? -1;

            // Remove the selected part data from the part table 
            if (selectedIndex >= 0 && selectedIndex < partData?.Count)
            {
                partData?.RemoveAt(selectedIndex);

                // Select the next item in the partData list 
                if (selectedIndex < partData?.Count)
                {
                    selectedPart = partData[selectedIndex];
                }
                else if (partData?.Count > 0)
                {
                    selectedPart = partData.Last();
                }
                else
                {
                    selectedPart = null;
                }
            }

            // Update pagination services
            RawPaginationService.SetItems(rawData ?? new List<MattelClass.SheetData>());
            PartPaginationService.SetItems(partData ?? new List<MattelClass.SheetData>());

            // Reset sorting state and icon for the raw table 
            RawSortingModel.SortStates["PartNumber"] = SortState.Default;
            RawSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";
            RawSortingModel.SortStates["Description"] = SortState.Default;
            RawSortingModel.SortIcons["Description"] = "icon_filter_darkGrey";

            // Clear selectedPart
            // selectedPart = null;

            // Force state update
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void Reset()
    {
        // Restore data from the original backup
        rawData = originalData.Select(data => new MattelClass.SheetData
        {
            ID = data.ID,
            PartNumber = data.PartNumber,
            Description = data.Description
        }).ToList();
        partData?.Clear();

        // Restore the original kit number 
        selectedKitNumber = originalKitNumber;

        // Update pagination services
        RawPaginationService.SetItems(rawData);
        PartPaginationService.SetItems(partData ?? new List<MattelClass.SheetData>());

        // Reset sorting state and icon for the raw & part table
        RawSortingModel.SortStates["PartNumber"] = SortState.Default;
        RawSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";
        RawSortingModel.SortStates["Description"] = SortState.Default;
        RawSortingModel.SortIcons["Description"] = "icon_filter_darkGrey";
        PartSortingModel.SortStates["PartNumber"] = SortState.Default;
        PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

        // Clear selections
        selectedRaw = null;
        selectedPart = null;

        // Force state update
        await InvokeAsync(StateHasChanged);
    }

    private async void Save()
    {
        if (!string.IsNullOrEmpty(selectedKitNumber) && selectedSheet != null && partData?.Any() == true)
        {
            var sheetList = MattelService.GetSheetList().DeepClone();

            // Check if the kit number is unique excluding the current selected sheet by ID
            var kitExists = sheetList.Sheet.Any(s =>
                s.KitNumber.Equals(selectedKitNumber, StringComparison.OrdinalIgnoreCase));

            selectedKitNumber = selectedKitNumber?.Replace(" ", string.Empty);

            if (kitExists)
            {
                // Alert the user to change the kit number
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = $"Kit number \"{selectedKitNumber}\" already exists.";
                popMsgData.Text_2 = "Please change the kit number.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => ClosePopup();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Set the new ID for the selected Kit if it's a new kit
            if (selectedSheet.ID != 0)
            {
                //selectedSheet.ID = sheetList.Sheet.Any() ? sheetList.Sheet.Max(s => s.ID) + 1 : 1;
                selectedSheet.ID = sheetList.Sheet.Any()
                ? sheetList.Sheet.Max(s => s.ID) + 1
                : 1;
            }

            // Set IDs for parts starting from 1
            for (int i = 0; i < partData.Count; i++)
            {
                partData[i].ID = i + 1;
            }

            selectedSheet.KitNumber = selectedKitNumber ?? string.Empty;
            selectedSheet.SheetData = partData;

            // Add new sheet entry since it didn't match an existing one
            sheetList.Sheet.Add(new MattelClass.Sheet
            {
                ID = selectedSheet.ID,
                KitNumber = selectedSheet.KitNumber ?? string.Empty,
                Description = selectedSheet.Description,
                SheetData = new List<MattelClass.SheetData>(selectedSheet.SheetData)
            });

            MattelService.SetSheetList(sheetList);

            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = $"New Kit created";
            popMsgData.Text_2 = "successfully !";
            popMsgData.Middle_Icon = "icon_sku_1";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnClick_1 = () => CloseModal();
            popMsgData.BtnText_2 = null!;
            popMsgData.BtnClick_2 = null!;

            // Clear other selections
            selectedPart = null;
            selectedRaw = null;
            selectedSheet = null;
            selectedKitNumber = null;
        }
    }

    private async void CloseModal()
    {
        await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
        NavigationManager.NavigateTo("Menu/MenuKit", true, true);
    }

    private async void ClosePopup()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private void SortRawItems()
    {
        var PartColumns = new List<string> { "PartNumber" };
        PartSortingModel = new SortingModel<MattelClass.SheetData>(PartColumns);

        var columns = new List<string> { "PartNumber", "Description" };
        RawSortingModel = new SortingModel<MattelClass.SheetData>(columns);
        try
        {
            if (rawData != null)
            {
                //Sorting for raw table
                RawSortingModel.SortStates["PartNumber"] = SortState.Descending;
                RawSortingModel.SortIcons["PartNumber"] = "icon_sortAsc_darkGrey";
                rawData = RawSortingModel.SortItems(rawData, "PartNumber");
                RawPaginationService.SetItems(rawData);
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

    private async Task SortRaw(string column)
    {
        try
        {
            if (column == "PartNumber" && rawData != null)
            {
                rawData = RawSortingModel.SortItems(rawData, column);
                RawPaginationService.SetItems(rawData); // Reset pagination
            }
            else if (column == "Description" && rawData != null)
            {
                rawData = RawSortingModel.SortItems(rawData, column);
                RawPaginationService.SetItems(rawData); // Reset pagination
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

        await InvokeAsync(StateHasChanged);
    }

    private async Task SortPart(string column)
    {
        try
        {
            if (column == "PartNumber" && partData != null)
            {
                partData = PartSortingModel.SortItems(partData, column);
                PartPaginationService.SetItems(partData); // Reset pagination
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

        await InvokeAsync(StateHasChanged);
    }

    private async void OnPageChanged()
    {
        // PartPaginationService.SetItems(new List<MattelClass.SheetData>());
        // PartPaginationService.GoToPage(1);
        var pagedRawData = rawData?.Skip((RawPaginationService.CurrentPage - 1) * RawPaginationService.ItemsPerPage)
                                    .Take(RawPaginationService.ItemsPerPage)
                                    .ToList();

        var pagedPartData = partData?.Skip((PartPaginationService.CurrentPage - 1) * PartPaginationService.ItemsPerPage)
                                    .Take(PartPaginationService.ItemsPerPage)
                                    .ToList();
        selectedRaw = null;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnPageChanged2()
    {
        selectedPart = null;
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
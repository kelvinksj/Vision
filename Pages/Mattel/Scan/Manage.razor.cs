using System.Resources;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Page;

namespace TaskManagerWeb.Components.Pages.Mattel.Scan;
public partial class Manage : IDisposable
{
    [Parameter]
    public int ID { get; set; }

    private string taskName = "Manage";
    private MessageLevel settingLevel = DebugParameters.MsgLvl_Manage;
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

    private string? originalKitNumber;
    private string? selectedKitNumber;
    private string? selectedPartNumber;
    private bool showModal = false;
    private string modalContent = string.Empty;
    private MattelClass.Sheet? dataKit;
    private MattelClass.SheetData? dataPart;
    private string KitButtonColor => selectedKitNumber == null
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : "background-color: #f36d33; border-color: #f36d33;";
    private string ScanBtnStyle => "background-color: #f36d33; border-color: #f36d33;";
    private string AddKitBtnStyle => (selectedRaw != null)
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";
    private string EditKitBtnStyle => (!string.IsNullOrWhiteSpace(selectedKitNumber))
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";
    private string AddPartBtnStyle => (!string.IsNullOrWhiteSpace(selectedKitNumber) && selectedRaw != null)
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";
    private string EditPartBtnStyle => (selectedPart != null)
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";
    private string RmvPartBtnStyle => (selectedPart != null)
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";
    private string ResetBtnStyle => (!string.IsNullOrWhiteSpace(selectedKitNumber) && (partData?.Any() == true))
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";

    private string SaveBtnStyle => (!string.IsNullOrWhiteSpace(selectedKitNumber) && (partData?.Any() == true))
        ? "background-color: #f36d33; border-color: #f36d33;"
        : "background-color: #a5a8a9; border-color: #a5a8a9; cursor: not-allowed;";

    private PopMsgData popMsgData = new();
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
    ~Manage()
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

            LoadRawData();
            RawPaginationService.ItemsPerPage = 8;
            PartPaginationService.SetItems(new List<MattelClass.SheetData>());
            PartPaginationService.ItemsPerPage = 8;

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
                $"UpdateInfoChange err:[{ex.HResult}]{ex.Message}",
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private async void LoadRawData()
    {
        var rawDataList = MattelService.GetScanSheetList();

        if (rawDataList.Sheet.Any())
        {
            selectedSheet = rawDataList.Sheet.FirstOrDefault(s => s.ID == ID);
            selectedKitNumber = selectedSheet?.KitNumber;
            rawData = selectedSheet?.SheetData?.ToList() ?? new List<MattelClass.SheetData>();
            // Store a copy of the original data and kit number 
            originalData = rawData.Select(data => new MattelClass.SheetData
            {
                ID = data.ID,
                PartNumber = data.PartNumber,
                Description = data.Description
            }).ToList();
            originalKitNumber = selectedKitNumber;

            if (rawData != null)
            {
                RawPaginationService.SetItems(rawData);
            }
        }

        await InvokeAsync(StateHasChanged);
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
            RawPaginationService.SetItems(rawData);
            PartPaginationService.SetItems(partData);

            // Reset sorting state and icon for the part table 
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

            // Clear selectedRaw
            // selectedRaw = null;

            // Ensure state is updated
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

    private async Task OnPartEdited(MattelClass.SheetData editedPart)
    {
        // Find the part being edited by its ID
        var part = partData?.FirstOrDefault(p => p.ID == editedPart.ID);

        // Check if the new part number already exists but isn't the same as the current part
        //var existingPart = partData?.FirstOrDefault(c => c.PartNumber == editedPart.PartNumber && c.ID != editedPart.ID);
        var existingPartData = partData?.FirstOrDefault(c => c.PartNumber == editedPart.PartNumber && c.ID != editedPart.ID);
        var existingRawData = rawData?.FirstOrDefault(c => c.PartNumber == editedPart.PartNumber);

        if (part != null)
        {
            if (existingPartData != null || existingRawData != null)
            {
                // Duplicate detected in either the first or second table
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "This part number already exists.";
                popMsgData.Text_2 = "Please change the part number.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseEditPartModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Update part details if no duplicate exists
            part.PartNumber = editedPart.PartNumber;
            part.Description = editedPart.Description;

            // Update the part numbers list for display
            if (partData != null)
                partNumbers = partData.Select(p => p.PartNumber).ToList();

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
        popMsgData.ActivatePopup = false; // Close the popup
        modalContent = "editPart"; // Keep the modal in edit mode
        showModal = true; // Reopen the modal
        await InvokeAsync(StateHasChanged);
    }

    private async void RmvPart()
    {
        if (selectedPart != null)
        {
            // Add the selected part data back to the raw table
            rawData?.Add(selectedPart);

            // Find the index of the selected part data
            int selectedIndex = partData?.IndexOf(selectedPart) ?? -1;

            // Remove the selected part data from the part table
            // partData?.Remove(selectedPart);

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
        if (!string.IsNullOrEmpty(selectedKitNumber) && partData != null && partData.Count > 0)
        {
            selectedKitNumber = selectedKitNumber.Replace(" ", string.Empty);
            var scanSheets = MattelService.GetScanSheetList();
            // Fetch existing sheets from JSON
            var existingSheets = MattelService.GetSheetList();
            // Fetch the selected sheet by ID 
            var selectedSheet = scanSheets.Sheet.FirstOrDefault(s => s.ID == this.ID);
            if (selectedSheet != null)
            {
                // Remove the sheet from scanSheets using ID 
                scanSheets.Sheet.Remove(selectedSheet);
            }

            // Check if the kit is new or existing
            var existingSheet = existingSheets.Sheet.FirstOrDefault(s => s.KitNumber.Equals
                (selectedKitNumber, StringComparison.OrdinalIgnoreCase));

            if (existingSheet != null)
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

            // Add new kit or update existing kit
            var newSheet = new MattelClass.Sheet
            {
                ID = existingSheets.Sheet.Count != 0 ? existingSheets.Sheet.Max(s => s.ID) + 1 : 1,
                KitNumber = selectedKitNumber,
                Description = "",
                SheetData = partData.Select((data, index) =>
                {
                    data.ID = index + 1; // Re-assign IDs sequentially starting from 1
                    return data;
                }).ToList()
            };

            existingSheets.Sheet.Add(newSheet);
            // Save updated lists to JSON 
            MattelService.SetSheetList(existingSheets);
            MattelService.SaveScanSheetList(scanSheets);

            // Display success message
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = $"New Kit created";
            popMsgData.Text_2 = "successfully !";
            popMsgData.Middle_Icon = "icon_sku_1";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnClick_1 = async () => await CloseModal(scanSheets, existingSheets);
            popMsgData.BtnText_2 = null!;
            popMsgData.BtnClick_2 = null!;

            await InvokeAsync(StateHasChanged);
        }
    }

    private async void ClosePopup()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CloseModal(MattelClass.SheetList scanSheets, MattelClass.SheetList existingSheets)
    {
        if (this.ID > 0)
        {
            RemoveKitFromScanAll(this.ID);
        }
        //await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
        NavigationManager.NavigateTo("/Mattel/Scan/ScanAll", true, true);
    }

    private async void RemoveKitFromScanAll(int id)
    {
        var scanSheets = MattelService.GetScanSheetList();
        // Remove the kit and its parts from the sheets list
        //var kitToRemove = scanSheets.Sheet.FirstOrDefault(s => s.KitNumber == kitNumber);
        var kitToRemove = scanSheets.Sheet.FirstOrDefault(s => s.ID == id);
        if (kitToRemove != null)
        {
            scanSheets.Sheet.Remove(kitToRemove);
            MattelService.SaveScanSheetList(scanSheets);
            KitPaginationService.SetItems(scanSheets.Sheet);
            await InvokeAsync(StateHasChanged);
        }
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/Mattel/Scan/ScanAll", true, true);
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

        await InvokeAsync(StateHasChanged);
    }

    private async void OnPageChanged2()
    {
        await Task.CompletedTask;
    }
}
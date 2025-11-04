using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel.Manual;
public partial class EditKit : IDisposable
{
    private string taskName = "EditKit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_EditKit;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private string? newKitNumber;
    private List<string> partNumbers = new();
    private List<MattelClass.SheetData> sheetDataList = new();
    private string? selectedKitNumber;
    private string? selectedPartNumber;
    private MattelClass.SheetList sheets = new();
    private MattelClass.Sheet? selectedSheet = new();
    private MattelClass.SheetData? data;
    private MattelClass.SheetData? selectedPart;
    [Parameter] public int ID { get; set; }
    private bool showModal = false;
    //private bool showEditModal = false;
    private bool showSuccessModal = false;
    private string modalContent = "";
    //private string? successContent;
    private bool disableSaveButton = true;
    private PopMsgData popMsgData = new();
    //private bool showRemoveModal = false;
    private string KitButtonColor => string.IsNullOrWhiteSpace(newKitNumber)
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : "background-color: #f36d33; border-color: #f36d33;";
    private string PartButtonColor => string.IsNullOrWhiteSpace(selectedPartNumber)
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : "background-color: #f36d33; border-color: #f36d33;";

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
    ~EditKit()
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

            sheets = MattelService.GetSheetList();
            PaginationService.ItemsPerPage = 8;
            selectedSheet = sheets.Sheet.FirstOrDefault(s => s.ID == ID);

            if (selectedSheet != null)
            {
                PaginationService.SetItems(selectedSheet.SheetData);
                selectedKitNumber = selectedSheet.KitNumber; // Set selectedKitNumber
                sheetDataList = selectedSheet.SheetData;
                partNumbers = selectedSheet.SheetData.Select(p => p.PartNumber).ToList();
            }

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

    private async void SelectRow(string Item)
    {
        selectedPartNumber = Item;
        selectedPart = sheetDataList.FirstOrDefault(p => p.PartNumber == Item);
        await InvokeAsync(StateHasChanged);
    }

    private string GetRow(string Item)
    {
        return
            (
                selectedPartNumber != null
                && selectedPartNumber == Item
            )
            ? "row-active"
            : string.Empty;
    }

    private async void CheckInput(ChangeEventArgs e)
    {
        selectedKitNumber = e.Value?.ToString()?.Replace(" ", string.Empty);
        disableSaveButton = false;
        await InvokeAsync(StateHasChanged);
    }

    private void ShowAddPartModal()
    {
        modalContent = "add";
        showModal = true;
    }

    private async void OnPartAdded(MattelClass.SheetData newPart)
    {
        if (newPart != null)
        {
            // Check if the part number already exists
            MattelClass.SheetData? existingPart = sheetDataList.FirstOrDefault
                (c => c.PartNumber == newPart.PartNumber);

            // If a part with the same part number already exists, display a validation message and do not add it
            if (existingPart != null)
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "The part number already exists.";
                popMsgData.Text_2 = "Please use a different part number.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseAddPartModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // If the part number is unique, proceed to add the new part
            newPart.ID = sheetDataList.Count != 0 ? sheetDataList.Max(p => p.ID) + 1 : 1;
            sheetDataList.Add(newPart);
            disableSaveButton = sheetDataList.Count == 0; // Enable Save button if there is data
            PaginationService.SetItems(sheetDataList);

            // Reset sorting state and icon for the part table 
            SortingModel.SortStates["PartNumber"] = SortState.Default;
            SortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

            await InvokeAsync(StateHasChanged);
            showModal = false;
        }
    }

    private async Task CloseAddPartModal()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged); // Close the modal
        modalContent = "add";
        showModal = true;
    }

    private async void ShowEditPartModal()
    {
        if (selectedPart != null)
        {
            // data = selectedPart;
            // Clone the selected part into a temporary object
            data = new MattelClass.SheetData
            {
                ID = selectedPart.ID,
                PartNumber = selectedPart.PartNumber,
                Description = selectedPart.Description
            };
            modalContent = "edit";
            showModal = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPartEdited(MattelClass.SheetData data)
    {
        // Find the part being edited by its ID
        var part = sheetDataList.FirstOrDefault(p => p.ID == data.ID);

        // Check if the new part number already exists but isn't the same as the current part
        var existingPart = sheetDataList.FirstOrDefault(c => c.PartNumber == data.PartNumber && c.ID != data.ID);

        if (part != null)
        {
            if (existingPart != null)
            {
                // Duplicate detected
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "The part number already exists.";
                popMsgData.Text_2 = "Please use a different part number.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseEditPartModal();
                popMsgData.BtnText_2 = null!; // No secondary button needed
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Update part details if no duplicate exists
            part.PartNumber = data.PartNumber;
            part.Description = data.Description;

            // Update the part numbers list for display
            partNumbers = sheetDataList.Select(p => p.PartNumber).ToList();

            // Enable or disable the Save button based on data
            disableSaveButton = sheetDataList.Count == 0;

            // Reset sorting state and icon for the part table 
            SortingModel.SortStates["PartNumber"] = SortState.Default;
            SortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

            // Reflect changes in the UI
            await InvokeAsync(StateHasChanged);

            // Close the modal after editing
            showModal = false;
        }
    }

    private async Task CloseEditPartModal()
    {
        popMsgData.ActivatePopup = false; // Close the popup
        modalContent = "edit"; // Keep the modal in edit mode
        showModal = true; // Reopen the modal
        await InvokeAsync(StateHasChanged);
    }

    private async void ShowRemovePartModal()
    {
        if (!string.IsNullOrEmpty(selectedPartNumber))
        {
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = "Are you sure you want to remove";
            popMsgData.Text_2 = "the selected part number?";
            popMsgData.Middle_Icon = "icon_remove";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnClick_1 = () => RemoveSelectedPart();
            popMsgData.BtnText_2 = "Cancel";
            popMsgData.BtnClick_2 = () => CloseRemoveModal();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void RemoveSelectedPart()
    {
        if (!string.IsNullOrEmpty(selectedPartNumber))
        {
            var partToRemove = sheetDataList.FirstOrDefault(p => p.PartNumber == selectedPartNumber);
            if (partToRemove != null)
            {
                // Find the index of the selected part
                int selectedIndex = sheetDataList.IndexOf(partToRemove);

                // Remove the selected part from the list
                sheetDataList.Remove(partToRemove);

                // Select the next item in the list 
                if (selectedIndex >= 0 && selectedIndex < sheetDataList.Count)
                {
                    selectedPartNumber = sheetDataList[selectedIndex].PartNumber;
                }
                else if (sheetDataList.Count > 0)
                {
                    selectedPartNumber = sheetDataList.Last().PartNumber;
                }
                else
                {
                    selectedPartNumber = null;
                }

                // Update part numbers list for display
                partNumbers = sheetDataList.Select(p => p.PartNumber).ToList();
                // selectedPartNumber = null;

                // Enable Save button if there is data
                disableSaveButton = sheetDataList.Count == 0;

                // Update pagination service
                PaginationService.SetItems(sheetDataList);

                // Force state update
                await InvokeAsync(StateHasChanged);
            }
        }
        // Reset sorting state and icon for the part table 
        SortingModel.SortStates["PartNumber"] = SortState.Default;
        SortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";

        CloseRemoveModal();
    }

    private async void CloseRemoveModal()
    {
        //showRemoveModal = false;
        // selectedPartNumber = null;
        await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void SaveKit()
    {
        if (selectedSheet != null)
        {
            // Fetch existing sheets from JSON
            var existingSheets = MattelService.GetSheetList();
            selectedKitNumber = selectedKitNumber?.Replace(" ", string.Empty);

            // Check if the kit number is unique (excluding the selected sheet if it exists)
            var kitExists = existingSheets.Sheet.Any(s => s.KitNumber.Equals(selectedKitNumber, StringComparison.OrdinalIgnoreCase) && s.ID != selectedSheet.ID);

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

            selectedSheet.KitNumber = selectedKitNumber ?? string.Empty; // Update KitNumber if changed
            selectedSheet.Description = ""; // Update the Description if needed
            //selectedSheet.SheetData = sheetDataList; // Update the part numbers
            selectedSheet.SheetData = sheetDataList.Select((data, index) =>
            {
                data.ID = index + 1; // Re-assign IDs sequentially starting from 1
                return data;
            }).ToList(); // Update the part numbers

            // Find the sheet in the list and update it
            var existingSheet = sheets.Sheet.FirstOrDefault(s => s.ID == selectedSheet.ID);
            if (existingSheet != null)
            {
                existingSheet.KitNumber = selectedSheet.KitNumber;
                existingSheet.Description = selectedSheet.Description;
                existingSheet.SheetData = selectedSheet.SheetData;

                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Kit updated";
                popMsgData.Text_2 = "successfully !";
                popMsgData.Middle_Icon = "icon_sku";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => Confirm();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
            }
            else
            {
                // Assign new ID if it's a new sheet
                selectedSheet.ID = sheets.Sheet.Count != 0 ? sheets.Sheet.Max(s => s.ID) + 1 : 1;
                sheets.Sheet.Add(selectedSheet);

                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Kit created";
                popMsgData.Text_2 = "successfully !";
                popMsgData.Middle_Icon = "icon_sku_1";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => Confirm();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
            }

            // Save the updated sheet list
            MattelService.SetSheetList(sheets);

            // Show success modal
            showSuccessModal = true;
            disableSaveButton = true; // Disable Save button after saving
            await InvokeAsync(StateHasChanged);
        }
    }

    private void Confirm()
    {
        // Navigate to SetupKit page
        NavigationManager.NavigateTo("/Mattel/Manual/SetupKit", true, true);
    }

    private async void ClosePopup()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private void Cancel()
    {
        // Discard changes
        newKitNumber = null;
        partNumbers.Clear();
        selectedPartNumber = null;
        selectedPart = null;
        disableSaveButton = true;

        // Navigate to SetupKit page
        NavigationManager.NavigateTo("/Mattel/Manual/SetupKit", true, true);
    }

    private async Task CloseModal()
    {
        showModal = false;
        //showEditModal = false; 
        await Task.CompletedTask;
    }

    private void ShowAddModalChanged(bool show)
    {
        showModal = show;
    }

    private void ShowEditModalChanged(bool show)
    {
        showModal = show;
    }

    private void ShowSuccessModalChanged(bool show)
    {
        showSuccessModal = show;
    }

    private async void OnPageChanged()
    {
        selectedPartNumber = null!;
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
            SortingModel.SortStates["PartNumber"] = SortState.Descending;
            SortingModel.SortIcons["PartNumber"] = "icon_sortAsc_darkGrey";

            //Perform initial sort
            sheetDataList = await SortingModel.SortItemsAsync(sheetDataList, "PartNumber");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(sheetDataList);
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
            sheetDataList = await SortingModel.SortItemsAsync(sheetDataList, column);
            PaginationService.SetItems(sheetDataList); // Reset pagination
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

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel.Scan;
public partial class AddKit : IDisposable
{
    private string taskName = "AddKit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ScanAddKit;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private string? newKitNumber;
    private List<string> partNumbers = new();
    private List<MattelClass.SheetData> sheetDataList = new();
    private string? selectedPartNumber;
    private MattelClass.SheetList sheets = new();
    private MattelClass.SheetData? data;
    private MattelClass.SheetData? selectedPart;

    private bool showModal = false;
    private bool showSuccessModal = false;
    private string modalContent = "";
    private bool disableSaveButton = true;
    private PopMsgData popMsgData = new();

    private string KitButtonColor => string.IsNullOrEmpty(newKitNumber) ? "background-color: #a5a8a9; border-color: #a5a8a9;" : "background-color: #f36d33; border-color: #f36d33;";
    private string PartButtonColor => string.IsNullOrEmpty(selectedPartNumber) ? "background-color: #a5a8a9; border-color: #a5a8a9;" : "background-color: #f36d33; border-color: #f36d33;";

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
    ~AddKit()
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

            sheets = MattelService.GetSheetList();

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

    private async void SelectPart(string partNumber)
    {
        selectedPartNumber = partNumber;
        selectedPart = sheetDataList.FirstOrDefault(p => p.PartNumber == partNumber);
        await InvokeAsync(StateHasChanged);
    }

    private async void CheckInput(ChangeEventArgs e)
    {
        newKitNumber = e.Value?.ToString();
        // Update the UI to re-evaluate button states
        await InvokeAsync(StateHasChanged);
    }

    private void ShowAddPartModal()
    {
        modalContent = "add";
        showModal = true;
    }

    private async void OnPartAdded(MattelClass.SheetData newPart)
    {
        newPart.ID = sheetDataList.Count != 0 ? sheetDataList.Max(p => p.ID) + 1 : 1;
        sheetDataList.Add(newPart);
        partNumbers.Add(newPart.PartNumber); // Maintain partNumbers for display in the table
        disableSaveButton = partNumbers.Count == 0; // Enable Save button if there is data
        await InvokeAsync(StateHasChanged);
        showModal = false;
    }

    private async void ShowEditPartModal()
    {
        if (selectedPart != null)
        {
            data = selectedPart;
            modalContent = "edit";
            showModal = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPartEdited(MattelClass.SheetData data)
    {
        var part = sheetDataList.FirstOrDefault(p => p.ID == data.ID);
        if (part != null)
        {
            part.PartNumber = data.PartNumber;
            part.Description = data.Description;
            partNumbers = sheetDataList.Select(p => p.PartNumber).ToList(); // Update part numbers list for display
            disableSaveButton = partNumbers.Count == 0; // Enable Save button if there is data
            await InvokeAsync(StateHasChanged);
        }
        await CloseModal();
    }

    private async void ShowRemovePartModal()
    {
        if (!string.IsNullOrEmpty(selectedPartNumber))
        {
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = $"Are you sure you want to remove";
            popMsgData.Text_2 = "the selected part number?";
            popMsgData.Middle_Icon = "icon_delete";
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
                sheetDataList.Remove(partToRemove);
                partNumbers = sheetDataList.Select(p => p.PartNumber).ToList(); // Update part numbers list for display
                selectedPartNumber = null;
                disableSaveButton = partNumbers.Count == 0; // Enable Save button if there is data
                await InvokeAsync(StateHasChanged);
            }
        }
        CloseRemoveModal();
    }

    private async void CloseRemoveModal()
    {
        //showRemoveModal = false;
        selectedPartNumber = null;
        await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void SaveKit()
    {
        if (!string.IsNullOrEmpty(newKitNumber) && sheetDataList.Count > 0)
        {
            // Fetch existing sheets from JSON
            var existingSheets = MattelService.GetSheetList();

            // Check if the kit is new or existing
            var existingSheet = existingSheets.Sheet.FirstOrDefault(s => s.KitNumber == newKitNumber);

            if (existingSheet == null)
            {
                // Add new kit and parts to the sheet list in memory
                var newSheet = new MattelClass.Sheet
                {
                    ID = existingSheets.Sheet.Count != 0 ? existingSheets.Sheet.Max(s => s.ID) + 1 : 1,
                    KitNumber = newKitNumber,
                    Description = "",
                    SheetData = sheetDataList
                };

                existingSheets.Sheet.Add(newSheet);
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = $"New Kit created";
                popMsgData.Text_2 = "successfully !";
                popMsgData.Middle_Icon = "icon_sku_1";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => Confirm();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
            }
            else
            {
                // Update existing sheet
                existingSheet.KitNumber = newKitNumber;
                existingSheet.Description = ""; // Update the Description if needed
                existingSheet.SheetData = sheetDataList;
            }

            // Save the updated list to JSON file
            MattelService.SetSheetList(existingSheets);

            disableSaveButton = true; // Disable Save button after saving
            await InvokeAsync(StateHasChanged);
        }
    }

    private void Confirm()
    {
        // Navigate to SetupKit page
        NavigationManager.NavigateTo("/SetupProgram/Manual/SetupKit", true, true);
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
        NavigationManager.NavigateTo("/SetupProgram/Manual/SetupKit", true, true);
    }

    private async Task CloseModal()
    {
        showModal = false;
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
}

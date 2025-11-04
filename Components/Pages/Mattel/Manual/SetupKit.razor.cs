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

namespace TaskManagerWeb.Components.Pages.Mattel.Manual;
public partial class SetupKit : IDisposable
{
    private string taskName = "SetupKit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_SetupKit;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;

    private MattelClass.SheetList sheets = new();
    private List<string> partNumbers = new();
    private MattelClass.SheetData data = new();
    private MattelClass.Sheet? selectedSheet;
    private string? selectedKitNumber;
    private string? selectedKitNumberToRemove;
    private string KitButtonColor => selectedKitNumber == null ? "background-color: #a5a8a9; border-color: #a5a8a9;" : "background-color: #f36d33; border-color: #f36d33;";
    private PopMsgData popMsgData = new();
    private List<MattelClass.Sheet> filteredKit = new();
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

        ResetState();
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
    ~SetupKit()
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
            filteredKit = new List<MattelClass.Sheet>(sheets.Sheet);
            PaginationService.ItemsPerPage = 8;
            PaginationService.SetItems(filteredKit);
            PartPaginationService.ItemsPerPage = 8;

            // Initial sort for both Kit and Part tables 
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

    private void Add()
    {
        NavigationManager.NavigateTo("/Mattel/Manual/AddKit", true, true);
    }

    private void Edit()
    {
        if (selectedSheet != null)
        {
            NavigationManager.NavigateTo($"/Mattel/Manual/EditKit/{selectedSheet.ID}", true, true);
        }
    }

    private void ResetState()
    {
        selectedKitNumber = null;
        data = new MattelClass.SheetData();
    }

    // private async void SelectKit(string kitNumber)
    // {
    //     selectedKitNumber = kitNumber;
    //     selectedSheet = sheets.Sheet.FirstOrDefault(s => s.KitNumber == kitNumber);
    //     selectedPart = null;
    //     partNumbers = selectedSheet?.SheetData.Select(p => p.PartNumber).ToList() ?? new List<string>(); // Update part numbers list
    //     await InvokeAsync(StateHasChanged);
    // }

    private async void SelectRow(string Item)
    {
        selectedKitNumber = Item;
        selectedSheet = sheets.Sheet.FirstOrDefault(s => s.KitNumber == Item);
        if (selectedSheet != null)
        {
            // Sort the part numbers in ascending order when a kit is selected 
            PartSortingModel.SortStates["PartNumber"] = SortState.Descending;
            PartSortingModel.SortIcons["PartNumber"] = "icon_sortAsc_darkGrey";
            var sortedParts = PartSortingModel.SortItems(selectedSheet.SheetData, "PartNumber");
            PartPaginationService.SetItems(sortedParts);
            PartPaginationService.GoToPage(1);
        }

        await InvokeAsync(StateHasChanged);
    }

    private string GetRow(string Item)
    {
        return
            (
                selectedKitNumber != null
                && selectedKitNumber == Item
            )
            ? "row-active"
            : string.Empty;
    }

    private async void ShowRemoveKitModal()
    {
        if (!string.IsNullOrEmpty(selectedKitNumber))
        {
            if (MattelService.Kit.IsKitUsed(selectedKitNumber))
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = $"The Kit \"{selectedKitNumber}\" is still being";
                popMsgData.Text_2 = $"use and cannot be deleted";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => CloseRemoveModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
            }
            else
            {
                selectedKitNumberToRemove = selectedKitNumber;
                // showRemoveModal = true;
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Are you sure you want";
                popMsgData.Text_2 = $"to Remove \"{selectedKitNumber}\" ?";
                popMsgData.Middle_Icon = "icon_remove";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => ConfirmRemoveKit();
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_2 = () => CloseRemoveModal();
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async void ConfirmRemoveKit()
    {
        if (!string.IsNullOrEmpty(selectedKitNumberToRemove))
        {
            var kitToRemove = sheets.Sheet.FirstOrDefault(s => s.KitNumber == selectedKitNumberToRemove);
            if (kitToRemove != null)
            {
                sheets.Sheet.Remove(kitToRemove);
                selectedKitNumberToRemove = null;
                // disableSaveButton = sheets.Sheet.Count == 0;
                await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
                popMsgData.ActivatePopup = false;
                filteredKit = new List<MattelClass.Sheet>(sheets.Sheet);
                PaginationService.SetItems(filteredKit);
                PartPaginationService.SetItems(new List<MattelClass.SheetData>());
                MattelService.SetSheetList(sheets);
                await InvokeAsync(StateHasChanged);
            }
        }

        CloseRemoveModal();
    }

    private async void CloseRemoveModal()
    {
        //showRemoveModal = false;
        selectedKitNumberToRemove = null;
        await jsRuntime.InvokeVoidAsync("eval", popMsgData.jsCommand);
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void FilterKit(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            filteredKit = new List<MattelClass.Sheet>(sheets.Sheet);
            selectedKitNumber = null;
            SortingModel.SortStates["KitNumber"] = SortState.Default;
            SortingModel.SortIcons["KitNumber"] = "icon_filter_darkGrey";
        }
        else
        {
            filteredKit = sheets.Sheet.Where(u =>
                u.KitNumber.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        filteredKit ??= new List<MattelClass.Sheet>();

        PaginationService.SetItems(filteredKit);
        PartPaginationService.SetItems([]);
        PartPaginationService.CurrentPage = 1;
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
            SortingModel.SortStates["KitNumber"] = SortState.Descending;
            SortingModel.SortIcons["KitNumber"] = "icon_sortAsc_darkGrey";
            filteredKit = await SortingModel.SortItemsAsync(filteredKit, "KitNumber");
            PaginationService.SetItems(filteredKit);

            //Sort for part table
            PartSortingModel.SortStates["PartNumber"] = SortState.Default;
            PartSortingModel.SortIcons["PartNumber"] = "icon_filter_darkGrey";
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
                filteredKit = await SortingModel.SortItemsAsync(filteredKit, column);
                PaginationService.SetItems(filteredKit); // Reset pagination
            }
            else if (column == "PartNumber" && selectedSheet != null)
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
        await Task.Delay(0);
    }
}
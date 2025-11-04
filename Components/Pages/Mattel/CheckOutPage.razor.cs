using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CheckOutPage : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private string taskName = "CheckOutPage";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_CheckOutPage;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private string workerNo = string.Empty;
    private ClaimsPrincipal authSetup = new ClaimsPrincipal();
    private List<MattelClass.Container> containers = [];
    private MattelClass.Container selectedData = null!;

    private bool disableBtn =>
        (
            selectedData == null
        );
    private string disableColor =>
        (
            selectedData == null
        )
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private PopMsgData popMsgData = new();
    private string filter = string.Empty;
    private MsgList mList = new();
    private static string areaCode = "2101";

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
            && !authUser.IsInRole("Operator")
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
                ContainerService.SubscribeQuery(UpdateQuery, PageGuid);
                TaskManagerService.SubscribeMessage(MsgUpdate, areaCode);
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
                    ContainerService.UnsubscribeQuery(UpdateQuery, PageGuid);
                    TaskManagerService?.UnsubscribeMessage(MsgUpdate, areaCode);

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
    ~CheckOutPage()
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

            containers = [];
            PaginationService.ItemsPerPage = 8;
            PaginationService.SetItems(containers);

            var columns = new List<string> { "KitId", "SkuId", "ContainerId", "Status", "IndexDateTime" };
            SortingModel = new SortingModel<MattelClass.Container>(columns);
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

    private void SelectRow(MattelClass.Container Data)
    {
        selectedData = Data;
    }

    private string GetRow(MattelClass.Container Data)
    {
        return (
            selectedData != null
            && selectedData.ID == Data.ID
        )
        ? "row-active"
        : string.Empty;
    }

    private async void OnPageChanged()
    {
        selectedData = null!;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnMouseClick()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnMouseClick triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // await JsInteropHelper.RemoveAllRangesAsync(jsRuntime);
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

    private async void OnClickFilter()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickFilter triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            filter = filter.Replace(" ", string.Empty);
            await ContainerService.AddFilterQueue(
                HttpClass.BehaviourType.CheckOut,
                filter,
                PageGuid
            );
            await JsInteropHelper.FocusMyHeader(jsRuntime);
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
            // Set initial sorting for the first column (KitId)
            SortingModel.SortStates["KitId"] = SortState.Descending;
            SortingModel.SortIcons["KitId"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var containersData = await SortingModel.SortItemsAsync(containers, "KitId");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(containersData);
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
            var containersData = await SortingModel.SortItemsAsync(containers, column);
            PaginationService.SetItems(containersData); // Reset pagination
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

    private void OnClickMap()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickMap triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            NavigationManager.NavigateTo("Mattel/VAS", true, true);
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

    private async void OnClickCheckout()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCheckout triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Confirm to checkout";
                popMsgData.Text_2 = "this item ?";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_1 = () => OnClickOk();
                popMsgData.BtnClick_2 = () => OnClickNok();
            }

            await JsInteropHelper.FocusMyHeader(jsRuntime);
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

    public async void OnClickOk()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCommit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            await InvokeAsync(StateHasChanged);

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = selectedData.LoadType,
                    LocationID = selectedData.Location,
                    KitID = selectedData.KitId,
                    SKUID = selectedData.SkuId,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.CheckOut
                }
            };

            await TaskManagerService.AddCommandQueue(httpUIData);
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

    public async void OnClickNok()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickNok triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            await JsInteropHelper.FocusMyHeader(jsRuntime);
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

    private async void UpdateQuery(List<HttpClass.ContainerData> ContainerDatas)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateQuery triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"UpdateQuery: ContainerDatas={CommonLib.JsonSerialize(ContainerDatas)}",
                settingLevel,
                MessageLevel.Trace
            );

            List<MattelClass.Container> newContainers = new();
            int count = 1;

            foreach (var item in ContainerDatas)
            {
                MattelClass.Container newRecord = new();
                CommonLib.CopyProperties(item, newRecord);

                if (newRecord.Condition == HttpClass.ConditionType.Quarantine)
                    newRecord.Status = HttpClass.ConditionType.Quarantine;

                newRecord.ID = count++;
                newContainers.Add(newRecord);
            }

            containers = newContainers;
            PaginationService.CurrentPage = 1;
            PaginationService.SetItems(containers);
            selectedData = null!;
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
            TaskManagerService?.AddCancelMessageQueue(TaskID);
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
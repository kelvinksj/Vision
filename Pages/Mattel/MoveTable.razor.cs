using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.RCS;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class MoveTable : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private string taskName = "MoveTable";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_MoveTable;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private ClaimsPrincipal authSetup = new ClaimsPrincipal();
    private Element.Table selectedData = null!;
    private List<Element.Table> tableList = [];
    private bool isDisplayMove = false;

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
    private MsgList mList = new();
    private static string areaCode = "9999";
    private string filter = string.Empty;
    private PopTableEditData popTableEditData = new();

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
    ~MoveTable()
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

            tableList = [];

            // tableList =
            // [
            //     new()
            //     {
            //         ID = 1,
            //         TableCode = "90000056",
            //         Location = "21010097",
            //         Status = Element.TableState.Loaded.ToString(),
            //         Type = Element.TableType.SKU.ToString(),
            //         KitNumber = "P-435632",
            //         PartNumber = "QWT456-DC243"
            //     },
            //     new()
            //     {
            //         ID = 2,
            //         TableCode = "90000045",
            //         Location = "21010072",
            //         Status = Element.TableState.Empty.ToString(),
            //         Type = Element.TableType.NA.ToString(),
            //         KitNumber = string.Empty,
            //         PartNumber = string.Empty
            //     },
            //     new()
            //     {
            //         ID = 3,
            //         TableCode = "90000042",
            //         Location = "21010066",
            //         Status = Element.TableState.Quarantine.ToString(),
            //         Type = Element.TableType.FG.ToString(),
            //         KitNumber = string.Empty,
            //         PartNumber = string.Empty
            //     }
            // ];

            PaginationService.ItemsPerPage = 8;
            PaginationService.SetItems(tableList);

            var columns = new List<string>
            {
                "TableCode",
                "Location",
                "Status",
                "Type",
                "KitNumber",
                "PartNumber"
            };
            SortingModel = new SortingModel<Element.Table>(columns);
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

    private void SelectRow(Element.Table Data)
    {
        selectedData = Data;
    }

    private string GetRow(Element.Table Data)
    {
        return
            (
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

    public async void OnClickCommit()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCommit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // if (checkInList != null && checkInList.List.Count > 0)
            // {
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = "Commit all completed";
            popMsgData.Text_2 = "records ?";
            popMsgData.Middle_Icon = "icon_issue";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnText_2 = "Cancel";
            popMsgData.BtnClick_1 = null!;
            popMsgData.BtnClick_2 = () => OnClickNok();
            // }

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
            // Set initial sorting for the first column (KitId) 
            SortingModel.SortStates["TableCode"] = SortState.Descending;
            SortingModel.SortIcons["TableCode"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var sortedData = await SortingModel.SortItemsAsync(tableList, "TableCode");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(sortedData);
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
            var sortedData = await SortingModel.SortItemsAsync(tableList, column);
            PaginationService.SetItems(sortedData);
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

    private async void OnClickMove()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickMove triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
                isDisplayMove = true;

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

    private async void OnClickUpdate()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null)
            {
                popTableEditData.IsVisible = true;
                popTableEditData.Header = "Update Table Info";
                popTableEditData.Table.TableCode = selectedData.TableCode;
                popTableEditData.Table.Location = selectedData.Location;
                popTableEditData.Table.Status = selectedData.Status;
                popTableEditData.Table.Type = selectedData.Type;
                popTableEditData.Table.KitNumber = selectedData.KitNumber;
                popTableEditData.Table.PartNumber = selectedData.PartNumber;
                popTableEditData.SetEventCallbacks(this, OnClickConfirmSave, OnClickCancelSave);
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

    private async Task OnClickConfirmSave()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickConfirmSave triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popTableEditData.IsVisible = false;
            selectedData.Status = popTableEditData.Table.Status;
            selectedData.Type = popTableEditData.Table.Type;
            selectedData.KitNumber = popTableEditData.Table.KitNumber;
            selectedData.PartNumber = popTableEditData.Table.PartNumber;
            await ContainerService.AddContainerCommand(selectedData);
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

    private async Task OnClickCancelSave()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCancelSave triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popTableEditData.IsVisible = false;
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

    private async void ChildUpdatePosition(RCSModel.NodeData NodeData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ChildUpdatePosition triggered",
            settingLevel,
            MessageLevel.Information
        );

        CommonLib.DisplayConsole(
            taskName,
            $"ChildUpdatePosition: Content={NodeData.Content}, Name={NodeData.Name}",
            settingLevel,
            MessageLevel.Trace
        );

        try
        {
            if (string.IsNullOrWhiteSpace(NodeData.Content))
            {
                isDisplayMove = false;
            }
            else
            {
                HttpClass.HttpTableCommand httpTableCommand = new()
                {
                    ID = string.Empty,
                    CallID = string.Empty,
                    Command = new()
                    {
                        CommandID = HttpClass.UICommand.Request,
                        TableCode = selectedData.TableCode,
                        Location = selectedData.Location,
                        MoveToPosition = NodeData.Content
                    }
                };
                _ = TaskManagerService.AddTableCommandQueue(httpTableCommand);
                isDisplayMove = false;
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
                HttpClass.BehaviourType.Maintenance,
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
                MessageLevel.Disable
            );

            tableList = new();
            int count = 1;
            foreach (var item in ContainerDatas)
            {
                tableList.Add(
                    new()
                    {
                        ID = count++,
                        TableCode = item.ContainerId,
                        Location = item.Location,
                        Status = (item.Condition == HttpClass.ConditionType.Quarantine)
                            ? HttpClass.ConditionType.Quarantine
                            : item.Status,
                        Type = item.LoadType,
                        KitNumber = item.KitId,
                        PartNumber = item.SkuId
                    }
                );
            }

            PaginationService.SetItems(tableList);
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
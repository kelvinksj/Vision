using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Models.RCS;
using TaskManagerWeb.Components.Service.DR;
using TaskManagerWeb.Components.Service.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class MoveRobot : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private string taskName = "MoveRobot";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_MoveRobot;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private ClaimsPrincipal authSetup = new ClaimsPrincipal();
    private Element.AMR selectedData = null!;
    private Element.AMR oldSelectedData = null!;
    private int oldPageNumber = 1;
    private List<Element.AMR> aMRList = [];
    private List<Element.AMR> filteredList = [];
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
    private string filterText = string.Empty;

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
                TaskManagerService.AMRSubscribe(UpdateAMRStatus, PageGuid);
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
                    TaskManagerService.AMRUnsubscribe(UpdateAMRStatus, PageGuid);
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
    ~MoveRobot()
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

            // aMRList =
            // [
            //     new()
            //     {
            //         ID = 1,
            //         Name = "AMR1",
            //         SerialNo = "CFGRH4324326D",
            //         Status = Element.AMRState.Init,
            //         Position = new()
            //         {
            //             Location = "20010001"
            //         },
            //         Battery = new()
            //         {
            //             Level = 80
            //         }
            //     },
            //     new()
            //     {
            //         ID = 2,
            //         Name = "AMR2",
            //         SerialNo = "JGFHTY324326D",
            //         Status = Element.AMRState.InTask,
            //         Position = new()
            //         {
            //             Location = "20020001"
            //         },
            //         Battery = new()
            //         {
            //             Level = 40
            //         }
            //     }
            // ];

            filteredList = aMRList;
            PaginationService.ItemsPerPage = 8;
            PaginationService.SetItems(filteredList);

            var columns = new List<string> { "Name", "SerialNo", "Status", "Position.Location", "Battery.Level" };
            SortingModel = new SortingModel<Element.AMR>(columns);
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

    private void SelectRow(Element.AMR Data)
    {
        selectedData = Data;
    }

    private string GetRow(Element.AMR Data)
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
            selectedData = null!;
            filterText = filter;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"Filter check \"{filter}\"",
                    settingLevel,
                    MessageLevel.Trace
                );

                filteredList = aMRList.Where(u =>
                    u.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.SerialNo.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.Status.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.Position.Location.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || u.Battery.Level.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            else
            {
                filteredList = aMRList;
            }

            PaginationService.SetItems(filteredList);
            PaginationService.CurrentPage = 1;
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
            // Set initial sorting for the first column
            SortingModel.SortStates["Name"] = SortState.Descending;
            SortingModel.SortIcons["Name"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            var sortedData = await SortingModel.SortItemsAsync(filteredList, "Name");

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
            var sortedData = await SortingModel.SortItemsAsync(filteredList, column);
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

    private async void OnClickSuspend()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickSuspend triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            HttpClass.HttpAMRCommand httpAMRCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    CommandID = HttpClass.UICommand.Suspend,
                    Name = selectedData.Name,
                    SerialNo = selectedData.SerialNo,
                    MoveToPosition = string.Empty
                }
            };
            _ = TaskManagerService.AddAMRCommandQueue(httpAMRCommand);
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

    private async void OnClickRestore()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickRestore triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            HttpClass.HttpAMRCommand httpAMRCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    CommandID = HttpClass.UICommand.Restore,
                    Name = selectedData.Name,
                    SerialNo = selectedData.SerialNo,
                    MoveToPosition = string.Empty
                }
            };
            _ = TaskManagerService.AddAMRCommandQueue(httpAMRCommand);
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
                HttpClass.HttpAMRCommand httpAMRCommand = new()
                {
                    ID = string.Empty,
                    CallID = string.Empty,
                    Command = new()
                    {
                        CommandID = HttpClass.UICommand.Move,
                        Name = selectedData.Name,
                        SerialNo = selectedData.SerialNo,
                        MoveToPosition = NodeData.Content
                    }
                };
                _ = TaskManagerService.AddAMRCommandQueue(httpAMRCommand);
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

    private async void UpdateAMRStatus(List<HttpClass.AMR> AMRs)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateAMRStatus triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"UpdateAMRStatus: ContainerDatas={CommonLib.JsonSerialize(AMRs)}",
                settingLevel,
                MessageLevel.Disable
            );

            oldSelectedData = selectedData;
            oldPageNumber = PaginationService.CurrentPage;
            int count = 1;

            if (aMRList.Count > 0)
                count = aMRList.Max(
                    n => n.ID
                ) + 1;

            foreach (var item in AMRs)
            {
                _ = int.TryParse(item.BatteryLevel, out int batLevel);
                var findAMR = aMRList!.FirstOrDefault(
                    n => n.Name == item.Name
                );

                if (findAMR == null)
                {
                    aMRList!.Add(
                        new()
                        {
                            ID = count++,
                            Name = item.Name,
                            SerialNo = item.SerialNo,
                            Status = item.Status,
                            Position = new()
                            {
                                Location = item.Position,
                                PosX = 0,
                                PosY = 0
                            },
                            Battery = new()
                            {
                                Level = batLevel,
                                Status = string.Empty
                            }
                        }
                    );
                }
                else
                {
                    findAMR.SerialNo = item.SerialNo;
                    findAMR.Status = item.Status;
                    findAMR.Position.Location = item.Position;
                    findAMR.Battery.Level = batLevel;
                }
            }

            List<Element.AMR> filteredList;

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                filteredList = aMRList.Where(u =>
                    u.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                    || u.SerialNo.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                    || u.Status.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                    || u.Position.Location.ToString().Contains(filterText, StringComparison.OrdinalIgnoreCase)
                    || u.Battery.Level.ToString().Contains(filterText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            else
            {
                filteredList = aMRList;
            }

            var sortedList = await SortingModel.ReSortItemsAsync(filteredList);
            PaginationService.SetItems(sortedList);

            if (oldSelectedData != null)
            {
                int recordIndex = PaginationService
                    .FindRecordIndexByClassInstance(
                        oldSelectedData,
                        [
                            "Name"
                        ]
                    );
                CommonLib.DisplayConsole(
                    taskName,
                    $"UpdateAMRStatus: Name={oldSelectedData.Name}",
                    settingLevel,
                    MessageLevel.Disable
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"UpdateAMRStatus: oldPageNumber={oldPageNumber}",
                    settingLevel,
                    MessageLevel.Disable
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"UpdateAMRStatus: recordIndex={recordIndex}",
                    settingLevel,
                    MessageLevel.Disable
                );

                if (recordIndex >= 0)
                {
                    selectedData = oldSelectedData;
                    PaginationService.GoPageNumber(recordIndex);
                }
            }
            else
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"UpdateAMRStatus: oldPageNumber={oldPageNumber}",
                    settingLevel,
                    MessageLevel.Disable
                );
                PaginationService.GoToPage(oldPageNumber);
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
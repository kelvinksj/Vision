using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CheckInMaintenance : IDisposable
{
    [CascadingParameter]
    private Guid PageGuid { get; set; }

    private static string taskName = "CheckInMaintenance";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_CheckInMaintenance;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private static IList<UserAccount> users = new List<UserAccount>();
    private IDisposable? registration;
    private static CommonLib? commonLib;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private readonly static object lockCheckInList = new();
    private static MattelClass.CheckInList checkInList = new();
    private readonly static object lockLineData = new();
    private static List<MattelClass.LineData> lineData = new();
    private PopMsgData popMsgData = new();
    private PopContainerData popContainerData = new();
    private MsgList mList = new();
    private string areaCode = "2001";
    private MattelClass.SheetList sheetList = new();
    private List<MattelClass.Sheet> filteredKits = [];
    private MattelClass.Sheet selectedSheet = new();
    private bool showKitDropdown = false;
    private string selectFontColor1 = string.Empty;
    private List<string> containerTypes =
    [
        "Empty",
        "SKU",
        "Gaylord"
    ];
    private string selectedType = string.Empty;
    private bool showTypeDropdown = false;
    private List<string> PartList = [];
    private List<string> filteredParts = [];
    private string selectedPart = string.Empty;
    private bool showPartDropdown = false;
    private string selectFontColor2 = string.Empty;

    private bool DisableSelectKit =>
        (
            selectedType == "Empty"
            || selectedType == "Gaylord"
        );
    private string StyleSelectKit =>
        DisableSelectKit
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool DisableSelectPart =>
        (
            selectedType == "Empty"
            || selectedType == "Gaylord"
        );
    private string StyleSelectPart =>
        DisableSelectPart
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

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
                ContainerService.SubscribeContainer(ContainerUpdate, areaCode);
                TaskManagerService.SubscribeMessage(MsgUpdate, areaCode);

                commonLib = CommonLib;

                try
                {
                    await jsRuntime.InvokeVoidAsync(
                        "JsFunctions.addKeyboardListenerEvent",
                        "JsKeyDown",
                        PageGuid.ToString()
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

                CommonLib.DisplayConsole(
                    taskName,
                    $"OnAfterRenderAsync ativate keyboard",
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

                    try
                    {
                        ContainerService.UnsubscribeContainer(ContainerUpdate, areaCode);
                        TaskManagerService?.UnsubscribeMessage(MsgUpdate, areaCode);

                        try
                        {
                            await jsRuntime.InvokeVoidAsync(
                                "JsFunctions.removeKeyboardListernerEvent"
                            );
                        }
                        catch { }
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
    ~CheckInMaintenance()
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

            lineData = new()
            {
                new(){
                    ID = 1,
                    Point = $"{areaCode}0001",
                    ContainerType = "Empty",
                },
                new(){
                    ID = 2,
                    Point = $"{areaCode}0002",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 3,
                    Point = $"{areaCode}0003",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 4,
                    Point = $"{areaCode}0004",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 5,
                    Point = $"{areaCode}0005",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 6,
                    Point = $"{areaCode}0006",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 7,
                    Point = $"{areaCode}0007",
                    ContainerType = "Empty"
                }
            };

            SetDynamicComponent(
                "CheckIn",
                lineData
            );

            sheetList = MattelService.GetSheetList();
            filteredKits = sheetList.Sheet;
            PartList = [];
            filteredParts = [];
            selectedType = "Empty";

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
        dynamic ParentVariable1
    )
    {
        try
        {
            var componentType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(ComponentName, StringComparison.OrdinalIgnoreCase));

            if (componentType != null)
            {
                var callback
                    = EventCallback.Factory.Create<dynamic>(this, value => UpdateFromChild(value));

                childCallback = EventCallback.Factory.Create(this, async () =>
                {
                    // Manually create an instance of the component
                    var childComponent = Activator.CreateInstance(componentType) as dynamic;

                    if (childComponent != null)
                    {
                        await childComponent.ChildMethod();
                        CommonLib.DisplayConsole(
                            taskName,
                            $"SetDynamicComponent childComponent.ChildMethod() triggered",
                            settingLevel,
                            MessageLevel.Information
                        );
                    }
                });

                dynamicComponent = builder =>
                {
                    builder.OpenComponent(0, componentType);
                    builder.AddAttribute(1, "ParentVariable1", ParentVariable1);
                    builder.AddAttribute(2, "ParentVariableChanged", callback);
                    builder.AddAttribute(3, "ChildCallback", childCallback);
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
            if (Data is MattelClass.LineData)
            {
                MattelClass.LineData lineData = Data;

                if (
                        (lineData.ContainerType == "Empty"
                        && (
                            (selectedType == "Empty"
                                && string.IsNullOrWhiteSpace(selectedSheet.KitNumber)
                                && string.IsNullOrWhiteSpace(selectedPart)
                            )
                            || (selectedType == "SKU"
                                && !string.IsNullOrWhiteSpace(selectedSheet.KitNumber)
                                && !string.IsNullOrWhiteSpace(selectedPart)
                            )
                        )
                    )
                    || (lineData.ContainerType == "Table"
                        && selectedType == "Gaylord"
                        && string.IsNullOrWhiteSpace(selectedSheet.KitNumber)
                        && string.IsNullOrWhiteSpace(selectedPart)
                    )
                )
                {
                    popContainerData.ActivatePopup = true;
                    popContainerData.IconClick = null!;
                    popContainerData.Text_1 = "Do you to checkin";
                    popContainerData.Text_2 = "new table ?";
                    popContainerData.Point = lineData.Point;
                    popContainerData.ContainerType = lineData.ContainerType;
                    popContainerData.BtnText_1 = "Ok";
                    popContainerData.BtnClick_1 = () => OnClickReplenish();
                    popContainerData.BtnText_2 = "Cancel";
                    popContainerData.BtnClick_2 = () => OnClickCancel();
                    popContainerData.BtnText_3 = null!;
                    popContainerData.BtnClick_3 = null!;
                    popContainerData.BtnText_4 = null!;
                    popContainerData.BtnClick_4 = null!;
                    await InvokeAsync(StateHasChanged);
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

    public async void OnClickReplenish()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickReplenish triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"UpdateRequest: Point={popContainerData.Point}, ContainerType={popContainerData.ContainerType}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = HttpClass.OrderLoadType.SKU,
                    LocationID = popContainerData.Point,
                    KitID = "NA",
                    SKUID = "NA",
                    Behaviour = "Any",
                    CommandID = HttpClass.UICommand.Clear
                }
            };

            if (selectedType == "SKU")
            {
                httpUIData.Command.KitID = selectedSheet.KitNumber;
                httpUIData.Command.SKUID = selectedPart;
                httpUIData.Command.Behaviour = string.Empty;
                httpUIData.Command.CommandID = HttpClass.UICommand.Request;
            }
            if (selectedType == "Gaylord")
                httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;

            await TaskManagerService.AddCommandQueue(httpUIData);
            popContainerData.ActivatePopup = false;
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

    public async void OnClickCancel()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCancel2 triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popContainerData.ActivatePopup = false;
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

    private async void ContainerUpdate(List<HttpClass.ContainerData> Containers)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ContainerUpdate triggered",
            settingLevel,
            MessageLevel.Disable
        );

        try
        {
            if (Containers?.Count > 0)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"ContainerUpdate: Containers={CommonLib.JsonSerialize(Containers)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                lock (lockLineData)
                {
                    foreach (var item in Containers)
                    {
                        var findData = lineData.FirstOrDefault(
                            n => n.Point[4..] == item.Location[4..]
                        );

                        if (findData != null)
                        {
                            if (item.ContainerId == string.Empty)
                                findData.ContainerType = "Empty";
                            else if (item.Status == "Empty")
                                findData.ContainerType = "Table";
                            else if (item.Status == "Loaded"
                                || item.Status == "Full"
                            )
                                findData.ContainerType = "Loaded";
                            else
                                findData.ContainerType = "NA";
                        }
                    }

                    SetDynamicComponent(
                        "CheckIn",
                        lineData
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

    private async void FilterKit(string filter)
    {
        showKitDropdown = true;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredKits = sheetList.Sheet
                .Where(u => u.KitNumber.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            selectFontColor1 = "color: rgba(0, 0, 0, 1)";
        }
        else
        {
            filteredKits = sheetList.Sheet;
            selectFontColor1 = "color: rgba(0, 0, 0, 0.6)";
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void ToggleKitDropdown()
    {
        if (!DisableSelectKit)
        {
            showKitDropdown = !showKitDropdown;
            showTypeDropdown = false;
            showPartDropdown = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void SelectKitNumber(MattelClass.Sheet Sheet)
    {
        showKitDropdown = false;
        selectedSheet = Sheet;
        PartList = selectedSheet.SheetData.Select(
            n => n.PartNumber
        ).ToList();
        filteredParts = PartList.DeepClone();
        selectedPart = string.Empty;

        await InvokeAsync(StateHasChanged);
    }

    private void HandleKitChange(string SelectedValue)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange1 triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // partNumbers = sheetList.Sheet.FirstOrDefault(
            //     n => n.KitNumber == SelectedValue
            // )!;

            // foreach (var item in orderKit.OrderKitData)
            // {
            //     item.PartNumber = string.Empty;
            //     item.Description = string.Empty;
            // }
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

    private async void ToggleTypeDropdown()
    {
        showTypeDropdown = !showTypeDropdown;
        showKitDropdown = false;
        showPartDropdown = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void SelectTypeNumber(string Type)
    {
        showTypeDropdown = false;
        selectedType = Type;

        if (selectedType == "Empty"
            || selectedType == "Gaylord"
        )
        {
            selectedSheet = new();
            selectedPart = string.Empty;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void FilterPart(string filter)
    {
        showPartDropdown = true;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredParts = PartList
                .Where(u => u.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            selectFontColor2 = "color: rgba(0, 0, 0, 1)";
        }
        else
        {
            filteredParts = PartList;
            selectFontColor2 = "color: rgba(0, 0, 0, 0.6)";
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void TogglePartDropdown()
    {
        if (!DisableSelectPart)
        {
            showPartDropdown = !showPartDropdown;
            showTypeDropdown = false;
            showKitDropdown = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void SelectPartNumber(string PartNumber)
    {
        showPartDropdown = false;
        selectedPart = PartNumber;
        await InvokeAsync(StateHasChanged);
    }

    private void HandlePartChange(string SelectedValue)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange1 triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // partNumbers = sheetList.Sheet.FirstOrDefault(
            //     n => n.KitNumber == SelectedValue
            // )!;

            // foreach (var item in orderKit.OrderKitData)
            // {
            //     item.PartNumber = string.Empty;
            //     item.Description = string.Empty;
            // }
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

    private void OnValidSubmit()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnValidSubmit triggered",
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
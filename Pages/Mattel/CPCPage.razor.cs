using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Specialized;
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
public partial class CPCPage : IDisposable
{
    private string taskName = "CPCPage";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_CPCPage;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private UserAccount user = new UserAccount();
    private IDisposable? registration;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private PopContainerData popContainerData = new();
    private MsgList mList = new();
    private string areaCode = "2300";
    private List<MattelClass.LineData> lineData = new();
    private MattelClass.CPCSettings cPCSettings = new();
    private List<MattelClass.LocationSetting> locSettings = [];
    private List<MattelClass.LocationSetting> oldLocSettings = [];
    private bool isLineEdit = false;
    private PopMsgData popMsgData = new();
    private bool isOperator = false;

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

        if (authUser.IsInRole("Operator"))
        {
            isOperator = true;
        }

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
                TaskManagerService?.SubscribeMessage(MsgUpdate, areaCode);
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
    ~CPCPage()
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
                    Point = $"{areaCode}0004",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 4,
                    Point = $"{areaCode}0005",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 5,
                    Point = $"{areaCode}0006",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 6,
                    Point = $"{areaCode}0007",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 7,
                    Point = $"{areaCode}0008",
                    ContainerType = "Empty",
                },
                new(){
                    ID = 8,
                    Point = $"{areaCode}0009",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 9,
                    Point = $"{areaCode}0010",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 10,
                    Point = $"{areaCode}0012",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 11,
                    Point = $"{areaCode}0013",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 12,
                    Point = $"{areaCode}0014",
                    ContainerType = "Empty"
                }
            };

            cPCSettings = MattelService.GetCPCSettings();
            locSettings = cPCSettings.LocationSettings.Where(
                n => n.LocationID[..4] == areaCode
            ).ToList();
            oldLocSettings = locSettings.DeepClone();

            SetDynamicComponent(
                "CPC",
                lineData,
                locSettings,
                isLineEdit
            );

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
        dynamic ParentVariable1,
        dynamic ParentVariable2,
        dynamic ParentVariable3
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SetDynamicComponent triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var componentType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(ComponentName, StringComparison.OrdinalIgnoreCase));

            if (componentType != null)
            {
                dynamicComponent = builder =>
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

                    builder.OpenComponent(0, componentType);
                    builder.AddAttribute(1, "ParentVariable1", ParentVariable1);
                    builder.AddAttribute(2, "ParentVariable2", ParentVariable2);
                    builder.AddAttribute(3, "ParentVariable3", ParentVariable3);
                    builder.AddAttribute(4, "ParentVariableChanged", callback);
                    builder.AddAttribute(5, "ChildCallback", childCallback);
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

    private Task UpdateFromChild(dynamic Data)
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

                popContainerData.ActivatePopup = true;
                popContainerData.IconClick = null!;
                popContainerData.Text_1 = "Confirm finished";
                popContainerData.Text_2 = "good removal ?";
                popContainerData.Point = lineData.Point;
                popContainerData.ContainerType = lineData.ContainerType;
                popContainerData.BtnText_1 = "OK";
                popContainerData.BtnClick_1 = () => OnClickOk();
                popContainerData.BtnText_2 = "Cancel";
                popContainerData.BtnClick_2 = () => OnClickCancel();
                popContainerData.BtnText_3 = null!;
                popContainerData.BtnClick_3 = null!;
                popContainerData.BtnText_4 = null!;
                popContainerData.BtnClick_4 = null!;
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

        return Task.CompletedTask;
    }

    public async void OnClickOk()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateRequest triggered",
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
                    LoadType = HttpClass.OrderLoadType.FG,
                    LocationID = popContainerData.Point,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Clear
                }
            };

            var result = await TaskManagerService.AddCommandQueue(httpUIData);
            popContainerData.ActivatePopup = false;
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

    public void OnClickCancel()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateClear triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popContainerData.ActivatePopup = false;
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
            MessageLevel.Information
        );

        try
        {
            if (Containers?.Count > 0)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"ContainerUpdate: Count={Containers.Count}",
                    settingLevel,
                    MessageLevel.Trace
                );

                foreach (var item in Containers)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"ContainerUpdate: Location={item.Location}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    var findData = lineData.FirstOrDefault(
                        n => n.Point == item.Location
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
                    "CPC",
                    lineData,
                    locSettings,
                    isLineEdit
                );

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

    private async void OnClickSetting()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickSetting triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            isLineEdit = true;

            SetDynamicComponent(
                "CPC",
                lineData,
                locSettings,
                isLineEdit
            );

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

    private async void OnClickSave()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickSave triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            isLineEdit = false;

            SetDynamicComponent(
                "CPC",
                lineData,
                locSettings,
                isLineEdit
            );

            var data1 = locSettings.Select(
                n => n.Parameter
            );
            var data2 = oldLocSettings.Select(
                n => n.Parameter
            );

            if (!data1.SequenceEqual(data2))
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Confirm to save ?";
                popMsgData.Text_2 = null!;
                popMsgData.Text_3 = null!;
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => OnClickOK();
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_2 = () => OnClickNok();
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

    private async void OnClickOK()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickOK triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            oldLocSettings = locSettings.DeepClone();
            MattelService.UpdateCPCLocations(locSettings);
            _ = ContainerService.AddSettingCommand();
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

    private async void OnClickNok()
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
            locSettings = oldLocSettings.DeepClone();

            SetDynamicComponent(
                "CPC",
                lineData,
                locSettings,
                isLineEdit
            );

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
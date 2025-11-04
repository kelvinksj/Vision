using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
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
public partial class LineRun : IDisposable
{
    private string taskName = "LineRun";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LineRun;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private UserAccount user = new UserAccount();
    private IDisposable? registration;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private MattelClass.MessageList msgList = new();
    private string selectFontColor = string.Empty;
    private MattelClass.ProductionList productionList = new();
    private MattelClass.LayoutList layoutList = new();
    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.SheetList sheetList = new();
    private MattelClass.LayoutData layoutData = new();
    private MattelClass.OrderKit orderKit = new();
    private MattelClass.ContainerData dragItem1 = new();
    private MattelClass.SheetData dragItem2 = new();
    private bool isEye = false;
    private bool isOrderKit = false;
    private bool isLiveRun = false;
    private MattelClass.ProductionData productionData = new();
    private PopLineData popLineData = new();
    private PopEndProdData popEndPRodData = new();
    private MsgList mList = new();
    private int lineNo = 0;
    private bool isOperator => lineNo != 0;
    private string oldLineString = string.Empty;

    private bool disableStart =>
        (
            string.Equals(productionData?.Status, "Open", StringComparison.OrdinalIgnoreCase) != true
        );
    private string disableStyleStart =>
        (
            disableStart
        )
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableEndProd =>
        (
            string.Equals(productionData?.Status, "Production", StringComparison.OrdinalIgnoreCase) != true
        );
    private string disableStyleEndProd =>
        (
            disableEndProd
        )
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

        if (authUser.IsInRole("Operator"))
        {
            var groupSidClaim = authUser.FindFirst(System.Security.Claims.ClaimTypes.GroupSid);

            if ((groupSidClaim?.Value[..1] ?? "") == "L")
                _ = int.TryParse(groupSidClaim?.Value[1..] ?? "0", out lineNo);
        }

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

                productionList = MattelService.GetProductionList();

                if (lineNo == 0)
                {
                    productionData = productionList.ProductionData.FirstOrDefault(
                        n => n.ID == 1
                    )!;
                }
                else
                {
                    productionData = productionList.ProductionData.FirstOrDefault(
                        n => n.ID == lineNo
                    )!;
                }

                orderKitList = MattelService.GetOrderKitList();

                if (productionData != null)
                {
                    oldLineString = productionData.LineNumber;
                    TaskManagerService.SubscribeMessage(MsgUpdate, oldLineString);
                    TaskManagerService.SubscribeTag(TagUpdate, oldLineString);
                    ContainerService.SubscribeContainer(ContainerUpdate, oldLineString);
                    orderKit = orderKitList.OrderKit.FirstOrDefault(
                       n => n.OrderKitNumber == productionData.OrderKitNumber
                   )!;
                }
                else
                {
                    orderKit = null!;
                }

                if (orderKit != null)
                {
                    layoutList = MattelService.GetLayoutList();
                    layoutData = layoutList.List.FirstOrDefault(
                        n => n.LayoutName == orderKit.LayoutName
                    )!;
                }
                else
                {
                    layoutData = null!;
                }

                dragItem1 = new();
                isEye = true;
                isOrderKit = true;
                dragItem2 = new();
                isLiveRun = true;

                if (orderKit != null && layoutData != null)
                    SetDynamicComponent(
                        layoutData.LayoutTypeName,
                        layoutData,
                        dragItem1,
                        isEye,
                        isOrderKit,
                        dragItem2,
                        orderKit,
                        isLiveRun
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
                    TaskManagerService.UnsubscribeMessage(MsgUpdate, oldLineString);
                    TaskManagerService.UnsubscribeTag(TagUpdate, oldLineString);
                    ContainerService.UnsubscribeContainer(ContainerUpdate, oldLineString);
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
    ~LineRun()
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
        dynamic ParentVariable3,
        dynamic ParentVariable4,
        dynamic ParentVariable5,
        dynamic ParentVariable6,
        dynamic ParentVariable7
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
                        = EventCallback.Factory.Create<(dynamic, dynamic)>(this, value => UpdateFromChild(value));

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
                    builder.AddAttribute(4, "ParentVariable4", ParentVariable4);
                    builder.AddAttribute(5, "ParentVariable5", ParentVariable5);
                    builder.AddAttribute(6, "ParentVariable6", ParentVariable6);
                    builder.AddAttribute(7, "ParentVariable7", ParentVariable7);
                    builder.AddAttribute(8, "ParentVariableChanged", callback);
                    builder.AddAttribute(9, "ChildCallback", childCallback);
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

    private Task UpdateFromChild((dynamic, dynamic) Data)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateFromChild triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var (param1, param2) = Data;

            if (string.Equals(productionData?.Status, "Open", StringComparison.OrdinalIgnoreCase) == true
                || string.Equals(productionData?.Status, "Production", StringComparison.OrdinalIgnoreCase) == true
            )
            {
                if (param1 is MattelClass.LineData && param2 is MattelClass.OrderKitData)
                {
                    MattelClass.LineData LineData = param1;
                    MattelClass.OrderKitData orderKitData = param2;

                    popLineData.ActivatePopup = true;
                    popLineData.IconClick = () => UpdateCancel();

                    if (LineData.ContainerType == "SKU")
                    {
                        popLineData.Text_1 = "Please click to \"Replenish\",";
                        popLineData.Text_2 = "\"Clear\" or \"Quarantine\"";
                    }
                    else
                    {
                        popLineData.Text_1 = "Please click to \"Replenish\",";
                        popLineData.Text_2 = "\"Last\" or \"Clear\"";
                    }

                    popLineData.BoxType(LineData.ContainerType);
                    popLineData.BoxText_1 = orderKitData.PartNumber;
                    popLineData.BoxText_2 = LineData.ContainerType;
                    popLineData.BoxText_3 = LineData.Point;
                    popLineData.BtnText_1 = "Replenish";
                    popLineData.BtnClick_1 = () => UpdateRequest();

                    if (LineData.ContainerType == "SKU")
                    {
                        popLineData.BtnText_2 = "Clear";
                        popLineData.BtnClick_2 = () => UpdateClear();
                    }
                    else
                    {
                        popLineData.BtnText_2 = "Last";
                        popLineData.BtnClick_2 = () => UpdateLast();
                    }

                    if (LineData.ContainerType == "SKU")
                    {
                        popLineData.BtnText_3 = "Quarantine";
                        popLineData.BtnClick_3 = () => UpdateQuarantine();
                    }
                    else
                    {
                        popLineData.BtnText_3 = "Clear";
                        popLineData.BtnClick_3 = () => UpdateClear();
                    }

                    popLineData.BtnText_4 = null!;
                    popLineData.BtnClick_4 = null!;
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

        return Task.CompletedTask;
    }

    public async void UpdateRequest()
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
               $"UpdateRequest: BoxText_1={popLineData.BoxText_1}, BoxText_2={popLineData.BoxText_2}, "
               + $"BoxText_3={popLineData.BoxText_3}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Request
                }
            };

            switch (popLineData.BoxText_2)
            {
                case "SKU":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.SKU;
                    httpUIData.Command.SKUID = popLineData.BoxText_1;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = orderKit.KitNumber;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                case "GL":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                case "FG":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.FG;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                default:
                    break;
            }

            popLineData.ActivatePopup = false;
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

    public async void UpdateClear()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateClear triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"UpdateClear: BoxText_1={popLineData.BoxText_1}, BoxText_2={popLineData.BoxText_2}, "
               + $"BoxText_3={popLineData.BoxText_3}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Clear
                }
            };

            switch (popLineData.BoxText_2)
            {
                case "SKU":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.SKU;
                    httpUIData.Command.SKUID = popLineData.BoxText_1;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = orderKit.KitNumber;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                case "GL":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                case "FG":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.FG;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                default:
                    break;
            }

            popLineData.ActivatePopup = false;
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

    public async void UpdateQuarantine()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateQuarantine triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"UpdateQuarantine: BoxText_1={popLineData.BoxText_1}, BoxText_2={popLineData.BoxText_2}, "
               + $"BoxText_3={popLineData.BoxText_3}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Quarantine
                }
            };

            switch (popLineData.BoxText_2)
            {
                case "SKU":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.SKU;
                    httpUIData.Command.SKUID = popLineData.BoxText_1;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = orderKit.KitNumber;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                default:
                    break;
            }

            popLineData.ActivatePopup = false;
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

    public async void UpdateLast()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateLast triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"UpdateLast: BoxText_1={popLineData.BoxText_1}, BoxText_2={popLineData.BoxText_2}, "
               + $"BoxText_3={popLineData.BoxText_3}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Last
                }
            };

            switch (popLineData.BoxText_2)
            {
                case "GL":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                case "FG":
                    httpUIData.Command.LoadType = HttpClass.OrderLoadType.FG;
                    httpUIData.Command.SKUID = string.Empty;
                    httpUIData.Command.LocationID = productionData.LineNumber + popLineData.BoxText_3;
                    httpUIData.Command.KitID = string.Empty;
                    await TaskManagerService.AddCommandQueue(httpUIData);
                    break;

                default:
                    break;
            }

            popLineData.ActivatePopup = false;
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

    public void UpdateCancel()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateCancel triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popLineData.ActivatePopup = false;
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

    private async Task HandleOnChange(string SelectedValue)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            productionList = MattelService.GetProductionList();
            productionData = productionList.ProductionData.FirstOrDefault(
                n => n.LineName == SelectedValue
            )!;
            orderKitList = MattelService.GetOrderKitList();
            mList = new();

            if (productionData != null)
            {
                TaskManagerService.UnsubscribeMessage(MsgUpdate, oldLineString);
                TaskManagerService.UnsubscribeTag(TagUpdate, oldLineString);
                ContainerService.UnsubscribeContainer(ContainerUpdate, oldLineString);

                oldLineString = productionData.LineNumber;
                TaskManagerService.SubscribeMessage(MsgUpdate, oldLineString);
                TaskManagerService.SubscribeTag(TagUpdate, oldLineString);
                ContainerService.SubscribeContainer(ContainerUpdate, oldLineString);

                orderKit = orderKitList.OrderKit.FirstOrDefault(
                   n => n.OrderKitNumber == productionData.OrderKitNumber
               )!;
            }
            else
            {
                orderKit = null!;
            }

            if (orderKit != null)
            {
                layoutList = MattelService.GetLayoutList();
                layoutData = layoutList.List.FirstOrDefault(
                    n => n.LayoutName == orderKit.LayoutName
                )!;
            }
            else
            {
                layoutData = null!;
            }

            dragItem1 = new();
            isEye = true;
            isOrderKit = true;
            dragItem2 = new();
            isLiveRun = true;

            if (orderKit != null && layoutData != null)
                SetDynamicComponent(
                    layoutData.LayoutTypeName,
                    layoutData,
                    dragItem1,
                    isEye,
                    isOrderKit,
                    dragItem2,
                    orderKit,
                    isLiveRun
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

        await Task.CompletedTask;
    }

    public async void OnClickStart()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickStart triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Request
                }
            };

            productionData.Status = "Production";
            MattelService.UpdateProductionList(productionData);
            MattelService.StatusOrderKit(
                productionData.OrderKitNumber,
                productionData.Status
            );
            StateHasChanged();

            foreach (var layoutItem in layoutData.LineData)
            {
                if (layoutItem.ContainerType == "SKU"
                    || layoutItem.ContainerType == "GL"
                    || layoutItem.ContainerType == "FG"
                )
                {
                    var orderData = orderKit.OrderKitData.FirstOrDefault(
                        n => n.Point == layoutItem.Point
                    );

                    if (orderData == null)
                        continue;

                    switch (layoutItem.ContainerType)
                    {
                        case "SKU":
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.SKU;
                            httpUIData.Command.SKUID = orderData.PartNumber;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = orderKit.KitNumber;
                            break;

                        case "GL":
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;
                            httpUIData.Command.SKUID = string.Empty;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = string.Empty;
                            break;

                        case "FG":
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.FG;
                            httpUIData.Command.SKUID = string.Empty;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = string.Empty;
                            break;

                        default:
                            break;
                    }

                    CommonLib.DisplayConsole(
                       taskName,
                       $"OnClickStart: httpUIData={CommonLib.JsonSerialize(httpUIData)}",
                       settingLevel,
                       MessageLevel.Disable
                    );

                    var result = await TaskManagerService.AddCommandQueue(httpUIData);
                }
            }

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

    public async void OnClickEndProd()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickEndProd triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popEndPRodData.ActivatePopup = true;
            popEndPRodData.IconClick = null!;
            popEndPRodData.Text_1 = "Do you want to confirm";
            popEndPRodData.Text_2 = "the end of production ?";
            popEndPRodData.Answer_1 = true;
            popEndPRodData.Answer_2 = false;
            popEndPRodData.Answer_3 = false;
            popEndPRodData.BtnText_1 = "Confirm";
            popEndPRodData.BtnClick_1 = () => OnClickOkEndProd();
            popEndPRodData.BtnText_2 = "Cancel";
            popEndPRodData.BtnClick_2 = () => OnClickNokEndProd();
            popEndPRodData.BtnText_3 = null!;
            popEndPRodData.BtnClick_3 = null!;
            popEndPRodData.BtnText_4 = null!;
            popEndPRodData.BtnClick_4 = null!;
            popEndPRodData.UpdateParent =
                (Answer1, Answer2, Answer3)
                => UpdateFromEndProd(Answer1, Answer2, Answer3);

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

    public async void OnClickOkEndProd()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickOkEndProd triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = string.Empty,
                    LocationID = string.Empty,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = string.Empty
                }
            };

            productionData.Status = "Completed";
            MattelService.UpdateProductionList(productionData);
            MattelService.StatusOrderKit(
                productionData.OrderKitNumber,
                productionData.Status
            );
            StateHasChanged();

            foreach (var layoutItem in layoutData.LineData)
            {
                if (layoutItem.ContainerType == "SKU"
                    || layoutItem.ContainerType == "GL"
                    || layoutItem.ContainerType == "FG"
                )
                {
                    var orderData = orderKit.OrderKitData.FirstOrDefault(
                        n => n.Point == layoutItem.Point
                    );

                    if (orderData == null)
                        continue;

                    bool result = false;

                    switch (layoutItem.ContainerType)
                    {
                        case "SKU":
                            httpUIData.Command.CommandID = popEndPRodData.Answer_1
                                ? HttpClass.UICommand.Clear : HttpClass.UICommand.Quarantine;
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.SKU;
                            httpUIData.Command.SKUID = orderData.PartNumber;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = orderKit.KitNumber;

                            result = await TaskManagerService.AddCommandQueue(httpUIData);
                            break;

                        case "GL":
                            if (!popEndPRodData.Answer_2)
                                break;

                            httpUIData.Command.CommandID = HttpClass.UICommand.Clear;
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.Gaylord;
                            httpUIData.Command.SKUID = string.Empty;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = string.Empty;

                            result = await TaskManagerService.AddCommandQueue(httpUIData);
                            break;

                        case "FG":
                            if (!popEndPRodData.Answer_3)
                                break;

                            httpUIData.Command.CommandID = HttpClass.UICommand.Clear;
                            httpUIData.Command.LoadType = HttpClass.OrderLoadType.FG;
                            httpUIData.Command.SKUID = string.Empty;
                            httpUIData.Command.LocationID = productionData.LineNumber + layoutItem.Point;
                            httpUIData.Command.KitID = string.Empty;

                            result = await TaskManagerService.AddCommandQueue(httpUIData);
                            break;

                        default:
                            break;
                    }
                }
            }

            popEndPRodData.ActivatePopup = false;
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

    public void OnClickNokEndProd()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickNokEndProd triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popEndPRodData.ActivatePopup = false;
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

    public async void UpdateFromEndProd(
        bool Answer_1,
        bool Answer_2,
        bool Answer_3
    )
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateFromEndProd triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popEndPRodData.Answer_1 = Answer_1;
            popEndPRodData.Answer_2 = Answer_2;
            popEndPRodData.Answer_3 = Answer_3;
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
            if (layoutData?.LineData?.Count > 0
                && Containers?.Count > 0
            )
            {
                var filterData = layoutData.LineData.Where(
                    n => !String.IsNullOrWhiteSpace(n.ContainerType)
                    && n.ContainerType != "NA"
                ).ToList();

                if (filterData != null)
                {
                    foreach (var item in filterData)
                    {
                        var findData = Containers.FirstOrDefault(
                            n => n.Location == $"{oldLineString}{item.Point}"
                        );
                        string containerType = string.Empty;
                        item.GotTableColor = string.Empty;

                        if (findData != null)
                        {
                            if (findData.ContainerId == string.Empty)
                            {
                                containerType = "Empty";
                                item.GotTableColor = string.Empty;
                            }
                            else if (findData.Status == "Empty")
                            {
                                containerType = "Table";
                                item.GotTableColor = "background-color: green; color: white";
                            }
                            else if (findData.Status == "Loaded"
                               || findData.Status == "Full"
                            )
                            {
                                containerType = "Loaded";
                                item.GotTableColor = "background-color: green; color: white";
                            }
                            else
                            {
                                containerType = "NA";
                                item.GotTableColor = string.Empty;
                            }
                        }
                    }

                    SetDynamicComponent(
                        layoutData.LayoutTypeName,
                        layoutData,
                        dragItem1,
                        isEye,
                        isOrderKit,
                        dragItem2,
                        orderKit,
                        isLiveRun
                    );

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

    private async void TagUpdate(List<string> Tags)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"TagUpdate triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            if (layoutData?.LineData?.Count > 0)
            {
                var filterData = layoutData.LineData.Where(
                    n => !String.IsNullOrWhiteSpace(n.ContainerType)
                    && n.ContainerType != "NA"
                ).ToList();

                if (filterData != null)
                {
                    foreach (var item in filterData)
                    {
                        var findData = Tags.FirstOrDefault(
                            n => n == $"{oldLineString}{item.Point}"
                        );
                        item.GotCommandColor = string.Empty;

                        if (findData != null)
                        {
                            item.GotCommandColor = "background-color: yellow; color: black";
                        }
                    }

                    SetDynamicComponent(
                        layoutData.LayoutTypeName,
                        layoutData,
                        dragItem1,
                        isEye,
                        isOrderKit,
                        dragItem2,
                        orderKit,
                        isLiveRun
                    );

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
    }
}
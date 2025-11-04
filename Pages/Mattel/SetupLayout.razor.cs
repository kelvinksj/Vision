using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class SetupLayout : IDisposable
{
    [Parameter]
    public int ID { get; set; } = 0;

    private string taskName = "SetupLayout";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_SetupLayout;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private bool isEye = false;
    private bool isOrderKit = false;
    private bool isLiveRun = false;
    private string selectFontColor = string.Empty;
    private string boxStyle = string.Empty;
    private string textStyle = string.Empty;
    private string newClass = string.Empty;
    private MattelClass.ContainerData dragItem1 = new();
    private MattelClass.SheetData dragItem2 = new();
    private MattelClass.ContainerData containerItem = new();
    private MattelClass.LayoutList layoutList = new();
    private MattelClass.LayoutData layoutData = new();
    private MattelClass.LayoutTypeList layoutTypeList = new();
    private MattelClass.ContainerTypeList containerTypeList = new();
    private MattelClass.OrderKit orderKit = new();
    private PopMsgData popMsgData = new();

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
        )
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        // CommonLib.DisplayConsole(
        //     taskName,
        //     $"OnAfterRenderAsync triggered",
        //     settingLevel,
        //     MessageLevel.Information
        // );

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
    ~SetupLayout()
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

            layoutList = MattelService.GetLayoutList();
            containerTypeList = MattelService.GetContainerTypeList();

            var tempList = MattelService.GetLayoutTypeList();
            layoutTypeList.LayoutTypeData = tempList.LayoutTypeData.OrderBy(
                n => n.LayoutTypeName
            ).ToList();

            if (ID <= 0)
            {
                layoutData.LayoutTypeName = "Standard";
                var findLayout = layoutTypeList.LayoutTypeData.FirstOrDefault(
                    n => n.LayoutTypeName == layoutData.LayoutTypeName
                );

                if (findLayout != null)
                {
                    var lineDataList = new List<MattelClass.LineData>();
                    for (int i = 0; i < findLayout.LocationQuantity; i++)
                    {
                        lineDataList.Add(new()
                        {
                            ID = i + 1,
                            Point = string.Empty,
                            Description = string.Empty,
                            ContainerType = containerTypeList.List[0].ContainerType,
                            BoxColor = containerTypeList.List[0].BoxColor,
                            FontColor = containerTypeList.List[0].FontColor,
                            CanDrop = true,
                            TableType = containerTypeList.List[0].ContainerType,
                            TableBoxColor = containerTypeList.List[0].BoxColor,
                            TableFontColor = containerTypeList.List[0].FontColor
                        });
                    }

                    if (layoutData.LayoutTypeName == "Standard")
                    {
                        int location = 21;

                        for (int i = 0; i < 13; i++)
                        {
                            lineDataList[i].Point = location.ToString("0000");
                            location++;
                        }

                        location = 41;

                        for (int i = 13; i < 26; i++)
                        {
                            lineDataList[i].Point = location.ToString("0000");
                            location++;
                        }

                        lineDataList[26].Point = "1001";
                        lineDataList[27].Point = "1002";
                        lineDataList[28].Point = "1003";
                        lineDataList[29].Point = "1004";
                    }
                    else if (layoutData.LayoutTypeName == "Island")
                    {
                        int location = 21;

                        for (int i = 0; i < 14; i++)
                        {
                            lineDataList[i].Point = location.ToString("0000");
                            location++;
                        }

                        location = 41;

                        for (int i = 14; i < 28; i++)
                        {
                            lineDataList[i].Point = location.ToString("0000");
                            location++;
                        }

                        lineDataList[28].Point = "1001";
                        lineDataList[29].Point = "1002";
                        lineDataList[30].Point = "1003";
                        lineDataList[31].Point = "1004";
                    }

                    layoutData.LineData = lineDataList;
                    var findFG = containerTypeList.List.FirstOrDefault(
                        n => n.ContainerType == "FG"
                    );

                    if (findFG != null)
                    {
                        for (int i = layoutData.LineData.Count - 3; i < layoutData.LineData.Count - 1; i++)
                        {
                            layoutData.LineData[i].ContainerType = findFG.ContainerType;
                            layoutData.LineData[i].BoxColor = findFG.BoxColor;
                            layoutData.LineData[i].FontColor = findFG.FontColor;
                        }
                    }
                }
            }
            else
            {
                layoutData = layoutList.List.FirstOrDefault(
                    n => n.ID == ID
                )!;
            }

            isOrderKit = false;
            dragItem2 = new();
            orderKit = new();
            isLiveRun = false;

            if (layoutData != null)
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

    private async void HandleOnChange(string SelectedValue)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var findLayout = layoutTypeList.LayoutTypeData.FirstOrDefault(
                n => n.LayoutTypeName == SelectedValue
            );

            if (findLayout != null)
            {
                var lineDataList = new List<MattelClass.LineData>();
                for (int i = 0; i < findLayout.LocationQuantity; i++)
                {
                    lineDataList.Add(new()
                    {
                        ID = i + 1,
                        Point = string.Empty,
                        Description = string.Empty,
                        ContainerType = containerTypeList.List[0].ContainerType,
                        BoxColor = containerTypeList.List[0].BoxColor,
                        FontColor = containerTypeList.List[0].FontColor,
                        CanDrop = true,
                        TableType = containerTypeList.List[0].ContainerType,
                        TableBoxColor = containerTypeList.List[0].BoxColor,
                        TableFontColor = containerTypeList.List[0].FontColor
                    });
                }

                if (SelectedValue == "Standard")
                {
                    int location = 21;

                    for (int i = 0; i < 13; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    location = 41;

                    for (int i = 13; i < 26; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    lineDataList[26].Point = "1001";
                    lineDataList[27].Point = "1002";
                    lineDataList[28].Point = "1003";
                    lineDataList[29].Point = "1004";
                }
                else if (SelectedValue == "Island")
                {
                    int location = 21;

                    for (int i = 0; i < 14; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    location = 41;

                    for (int i = 14; i < 28; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    lineDataList[28].Point = "1001";
                    lineDataList[29].Point = "1002";
                    lineDataList[30].Point = "1003";
                    lineDataList[31].Point = "1004";
                }

                layoutData.LineData = lineDataList;
                var findFG = containerTypeList.List.FirstOrDefault(
                    n => n.ContainerType == "FG"
                );

                if (findFG != null)
                {
                    for (int i = layoutData.LineData.Count - 3; i < layoutData.LineData.Count - 1; i++)
                    {
                        layoutData.LineData[i].ContainerType = findFG.ContainerType;
                        layoutData.LineData[i].BoxColor = findFG.BoxColor;
                        layoutData.LineData[i].FontColor = findFG.FontColor;
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

    private void OnValidSubmit(MattelClass.LayoutData LayoutData)
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

    private async Task UpdateFromChild((dynamic, dynamic) Data)
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

            if (param1 is MattelClass.LayoutData)
            {
                layoutData = param1;
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

        return;
    }

    private async Task CallChildMethod()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"CallChildMethod triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (childCallback.HasDelegate)
            {
                await childCallback.InvokeAsync(null);
                CommonLib.DisplayConsole(
                    taskName,
                    $"CallChildMethod childCallback.InvokeAsync triggered",
                    settingLevel,
                    MessageLevel.Information
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

        await Task.CompletedTask;
    }

    private async void HandleOnMouseDown()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnMouseDown triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.RemoveAllRangesAsync(jsRuntime);
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

    private async void HandleDragStart(MattelClass.ContainerData ContainerData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleDragStart triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            dragItem1 = ContainerData;
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
            await jsRuntime.InvokeVoidAsync("setDragData", ContainerData);

            CommonLib.DisplayConsole(
                taskName,
                $"HandleDragStart dragItem1={CommonLib.JsonSerialize(dragItem1)}",
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

    private void HandleDragEnter()
    {

    }

    private void HandleDrop(ref MattelClass.ContainerData ContainerData)
    {
        ContainerData = dragItem1;
        StateHasChanged();
    }

    private void HandleDragLeave()
    {

    }

    private async void HandleOnClickEye()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnClickEye triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            isEye = !isEye;
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
            // await CallChildMethod();
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

    // To check layout name validity
    private bool IsLayoutNameValid(string layoutName)
    {
        return
            (
                !string.IsNullOrEmpty(layoutName)
                && !string.IsNullOrWhiteSpace(layoutName)
            );
    }

    private bool HasRequiredTypes(IEnumerable<MattelClass.LineData> lineData)
    {
        bool hasSKU = lineData.Any(ld => ld.ContainerType == "SKU");
        bool hasFG = lineData.Any(ld => ld.ContainerType == "FG");
        return hasSKU && hasFG;
    }

    public void OnClickAdd()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickAdd triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnClickAdd ID={ID}, layoutData=",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"{CommonLib.JsonSerialize(layoutData)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (ID == -1 && layoutData != null)
            {
                if (IsLayoutNameValid(layoutData.LayoutName))
                {
                    if (!MattelService.IsDuplicateLayoutName(layoutData))
                    {
                        if (HasRequiredTypes(layoutData.LineData))
                        {
                            CommonLib.DisplayConsole(
                                taskName,
                                $"OnClickAdd data added",
                                settingLevel,
                                MessageLevel.Information
                            );

                            if (layoutList != null)
                            {
                                if (layoutList.List.Count == 0)
                                {
                                    layoutData.ID = 1;
                                }
                                else
                                {
                                    layoutData.ID = layoutList.List.Max(n => n.ID) + 1;
                                }
                            }

                            popMsgData.ActivatePopup = true;
                            popMsgData.Text_1 = "New Layout created";
                            popMsgData.Text_2 = "successfully !";
                            popMsgData.Middle_Icon = "icon_modular";
                            popMsgData.BtnText_1 = "OK";
                            popMsgData.BtnClick_1 = () => ClickOk();
                            popMsgData.BtnText_2 = null!;
                            popMsgData.BtnClick_2 = null!;
                        }
                        else
                        {
                            popMsgData.ActivatePopup = true;
                            popMsgData.Text_1 = "Layout must contain at least";
                            popMsgData.Text_2 = "one SKU and one FG";
                            popMsgData.Middle_Icon = "icon_issue";
                            popMsgData.BtnText_1 = "OK";
                            popMsgData.BtnClick_1 = () => OnClickOk();
                        }
                    }
                    else
                    {
                        popMsgData.ActivatePopup = true;
                        popMsgData.Text_1 = $"Layout name \"{layoutData.LayoutName}\"";
                        popMsgData.Text_2 = "already exist";
                        popMsgData.Middle_Icon = "icon_issue";
                        popMsgData.BtnText_1 = "OK";
                        popMsgData.BtnClick_1 = () => OnClickOk();
                        popMsgData.BtnText_2 = null!;
                        popMsgData.BtnClick_2 = null!;
                    }
                }
                else
                {
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = "Please enter a valid layout name";
                    popMsgData.Text_2 = "";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
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

    public void OnClickSave()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickSave triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnClickSave ID={ID}, layoutData=",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"{CommonLib.JsonSerialize(layoutData)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (ID > 0 && layoutData != null)
            {
                if (HasRequiredTypes(layoutData.LineData))
                {
                    var index = layoutList.List.FindIndex(
                        n => n.ID == layoutData.ID
                    );

                    if (index != -1)
                    {
                        CommonLib.DisplayConsole(
                            taskName,
                            $"OnClickSave data saved",
                            settingLevel,
                            MessageLevel.Information
                        );

                        layoutList.List[index] = layoutData;
                        popMsgData.ActivatePopup = true;
                        popMsgData.Text_1 = "Layout updated";
                        popMsgData.Text_2 = "successfully !";
                        popMsgData.Middle_Icon = "icon_layout";
                        popMsgData.BtnText_1 = "OK";
                        popMsgData.BtnClick_1 = () => Ok();
                        popMsgData.BtnText_2 = null!;
                        popMsgData.BtnClick_2 = null!;
                    }
                    else
                    {
                        popMsgData.ActivatePopup = true;
                        popMsgData.Text_1 = $"Layout name \"{layoutData.LayoutName}\"";
                        popMsgData.Text_2 = "data error";
                        popMsgData.Middle_Icon = "icon_issue";
                        popMsgData.BtnText_1 = "OK";
                        popMsgData.BtnClick_1 = () => OnClickOk();
                        popMsgData.BtnText_2 = null!;
                        popMsgData.BtnClick_2 = null!;
                    }
                }
                else
                {
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = "Layout must contain at least";
                    popMsgData.Text_2 = "one SKU and one FG";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
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

    private void OnClickClear()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickClear triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            layoutList = MattelService.GetLayoutList();
            containerTypeList = MattelService.GetContainerTypeList();

            var tempList = MattelService.GetLayoutTypeList();
            layoutTypeList.LayoutTypeData = tempList.LayoutTypeData.OrderBy(
                n => n.LayoutTypeName
            ).ToList();

            layoutData.LayoutTypeName = "Standard";
            var findLayout = layoutTypeList.LayoutTypeData.FirstOrDefault(
                n => n.LayoutTypeName == layoutData.LayoutTypeName
            );

            if (findLayout != null)
            {
                var lineDataList = new List<MattelClass.LineData>();
                for (int i = 0; i < findLayout.LocationQuantity; i++)
                {
                    lineDataList.Add(new()
                    {
                        ID = i + 1,
                        Point = string.Empty,
                        Description = string.Empty,
                        ContainerType = containerTypeList.List[0].ContainerType,
                        BoxColor = containerTypeList.List[0].BoxColor,
                        FontColor = containerTypeList.List[0].FontColor,
                        CanDrop = true,
                        TableType = containerTypeList.List[0].ContainerType,
                        TableBoxColor = containerTypeList.List[0].BoxColor,
                        TableFontColor = containerTypeList.List[0].FontColor
                    });
                }

                if (layoutData.LayoutTypeName == "Standard")
                {
                    int location = 21;

                    for (int i = 0; i < 13; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    location = 41;

                    for (int i = 13; i < 26; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    lineDataList[26].Point = "1001";
                    lineDataList[27].Point = "1002";
                    lineDataList[28].Point = "1003";
                    lineDataList[29].Point = "1004";
                }
                else if (layoutData.LayoutTypeName == "Island")
                {
                    int location = 21;

                    for (int i = 0; i < 14; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    location = 41;

                    for (int i = 14; i < 28; i++)
                    {
                        lineDataList[i].Point = location.ToString("0000");
                        location++;
                    }

                    lineDataList[28].Point = "1001";
                    lineDataList[29].Point = "1002";
                    lineDataList[30].Point = "1003";
                    lineDataList[31].Point = "1004";
                }

                layoutData.LineData = lineDataList;
                var findFG = containerTypeList.List.FirstOrDefault(
                    n => n.ContainerType == "FG"
                );

                if (findFG != null)
                {
                    for (int i = layoutData.LineData.Count - 3; i < layoutData.LineData.Count - 1; i++)
                    {
                        layoutData.LineData[i].ContainerType = findFG.ContainerType;
                        layoutData.LineData[i].BoxColor = findFG.BoxColor;
                        layoutData.LineData[i].FontColor = findFG.FontColor;
                    }
                }
            }

            isOrderKit = false;
            dragItem2 = new();
            orderKit = new();
            isLiveRun = false;
            isEye = false;

            if (layoutData != null)
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

    private void OnClickOk()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOk triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
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

    private void ClickOk()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOk triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            MattelService.AddLayoutData(layoutData);
            NavigationManager.NavigateTo("Mattel/LayoutList", true, true);
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

    private void Ok()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOk triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            MattelService.EditLayoutData(layoutData);
            NavigationManager.NavigateTo("Mattel/LayoutList", true, true);
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
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
public partial class QuickSetup : IDisposable
{
    private string taskName = "QuickSetup";
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
    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.SheetList sheetList = new();
    private MattelClass.Sheet partNumbers = new();
    private List<MattelClass.Sheet> filteredKitNumbers = new();
    private List<MattelClass.LayoutTypeData> filteredLayoutType = new();
    private PopMsgData popMsgData = new();
    private bool showKitDropdown = false;
    private bool showLayoutDropdown = false;
    private bool IsCreateButtonEnabled { get; set; } = false;

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
    ~QuickSetup()
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

            layoutList = MattelService.GetLayoutList();
            containerTypeList = MattelService.GetContainerTypeList();

            var tempList1 = MattelService.GetSheetList();
            var tempList2 = new MattelClass.SheetList();

            orderKitList = MattelService.GetOrderKitList();

            foreach (var item in tempList1.Sheet)
            {
                var findData = orderKitList.OrderKit.FirstOrDefault(
                    n => n.KitNumber == item.KitNumber
                );

                if (findData == null)
                    tempList2.Sheet.Add(item);
            }

            sheetList.Sheet = tempList2.Sheet.OrderBy(
                n => n.KitNumber
            ).ToList();

            filteredKitNumbers = sheetList.Sheet;

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

                foreach (var item in layoutData.LineData)
                {
                    orderKit.OrderKitData.Add(new()
                    {
                        ID = item.ID,
                        Point = item.Point,
                        PartNumber = string.Empty,
                        Description = string.Empty,
                        CanDrop = true
                    });
                }
            }

            filteredLayoutType = layoutTypeList.LayoutTypeData;

            isOrderKit = false;
            dragItem2 = new();
            // orderKit = new();
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

            IsCreateButtonEnabled = AreAllSKUsAssigned();

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

                orderKit.OrderKitData = new();

                foreach (var item in layoutData.LineData)
                {
                    orderKit.OrderKitData.Add(new()
                    {
                        ID = item.ID,
                        Point = item.Point,
                        PartNumber = string.Empty,
                        Description = string.Empty,
                        CanDrop = true
                    });
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

    private void HandleOnChange2(string SelectedValue)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            partNumbers = sheetList.Sheet.FirstOrDefault(
                n => n.KitNumber == SelectedValue
            )!;

            foreach (var item in orderKit.OrderKitData)
            {
                item.PartNumber = string.Empty;
                item.Description = string.Empty;
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

    private async void ToggleKitDropdown()
    {
        showKitDropdown = !showKitDropdown; ;
        showLayoutDropdown = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void ToggleLayoutDropdown()
    {
        showLayoutDropdown = !showLayoutDropdown; ;
        showKitDropdown = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void SelectKitNumber(string kitNumber)
    {
        orderKit.KitNumber = kitNumber;
        showKitDropdown = false;
        HandleOnChange2(kitNumber);
        await InvokeAsync(StateHasChanged);
    }

    private async void SelectLayoutType(string layoutType)
    {
        layoutData.LayoutTypeName = layoutType;
        showLayoutDropdown = false;
        HandleOnChange(layoutType);
        await InvokeAsync(StateHasChanged);
    }

    private async void FilterKit(string filter)
    {
        showKitDropdown = true;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredKitNumbers = sheetList.Sheet
                .Where(u => u.KitNumber.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            selectFontColor = "color: rgba(0, 0, 0, 1)";
        }
        else
        {
            filteredKitNumbers = sheetList.Sheet;
            selectFontColor = "color: rgba(0, 0, 0, 0.6)";
        }

        await InvokeAsync(StateHasChanged);
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

    private void OnValidSubmit2(MattelClass.OrderKit orderKit)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnValidSubmit2 triggered",
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
                IsCreateButtonEnabled = AreAllSKUsAssigned();
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
                // await childCallback.InvokeAsync(isEye);
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

    private async void HandleDragStart2(MattelClass.SheetData SheetData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleDragStart triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            dragItem2 = SheetData;
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
            await jsRuntime.InvokeVoidAsync("setDragData", SheetData);

            CommonLib.DisplayConsole(
                taskName,
                $"HandleDragStart dragItem2={CommonLib.JsonSerialize(dragItem2)}",
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
            isOrderKit = !isOrderKit;
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

    private bool AreAllSKUsAssigned()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"AreAllSKUsAssigned triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Find all LineData with ContainerType "SKU"
            var skuLineData = layoutData.LineData.Where(ld => ld.ContainerType == "SKU").ToList();

            if (skuLineData.Count == 0)
            {
                return false;
            }

            // Check if all SKUs in LineData have corresponding OrderKitData with a PartNumber
            foreach (var lineData in skuLineData)
            {
                var orderKitData = orderKit.OrderKitData.FirstOrDefault(
                    n => n.Point == lineData.Point
                );

                if (orderKitData == null || string.IsNullOrEmpty(orderKitData.PartNumber))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );

            // Add return statement here to handle exceptions
            return false;
        }
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
                $"{CommonLib.JsonSerialize(layoutData)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (layoutData != null)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"OnClickAdd data added",
                    settingLevel,
                    MessageLevel.Information
                );

                if (orderKitList != null)
                {
                    if (orderKitList.OrderKit.Count == 0)
                    {
                        orderKit.ID = 1;
                    }
                    else
                    {
                        orderKit.ID = orderKitList.OrderKit.Max(n => n.ID) + 1;
                    }

                    var existingLayoutNames = orderKitList.OrderKit.Select(kit => kit.LayoutName).ToList();
                    orderKit.LayoutName = GenerateNextLayoutName(existingLayoutNames);
                }

                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Kit Linked with Layout";
                popMsgData.Text_2 = "successfully !";
                popMsgData.Middle_Icon = "icon_web_link";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => ClickOk();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
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

    private string GenerateNextLayoutName(List<string> existingLayoutNames)
    {
        string prefix = "QS";
        int nextNumber = 1;

        if (existingLayoutNames.Any())
        {
            var lastLayoutName = existingLayoutNames
                .Where(name => name.StartsWith(prefix))
                .OrderByDescending(name => name)
                .FirstOrDefault();

            if (lastLayoutName != null)
            {
                _ = int.TryParse(lastLayoutName.Substring(prefix.Length), out int lastNumber);
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private void OnClickClear()
    {
        orderKit = new MattelClass.OrderKit();
        layoutData = new MattelClass.LayoutData();
        newClass = string.Empty;
        isEye = false;
        partNumbers = new MattelClass.Sheet();
        UpdateInfoChange();
        StateHasChanged();
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
            orderKit.OrderKitNumber = orderKit.KitNumber;
            orderKit.Status = "Open";
            orderKit.CreatedData = DateTime.Now;
            MattelService.AddOrderKit(orderKit);
            layoutData.LayoutName = orderKit.LayoutName;
            MattelService.AddLayoutData(layoutData);
            NavigationManager.NavigateTo("Mattel/OrderKitList", true, true);
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
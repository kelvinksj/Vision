using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class SetupOrderKit : IDisposable
{
    [Parameter]
    public int ID { get; set; } = 0;

    private string taskName = "SetupOrderKit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_SetupOderKit;
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
    private MattelClass.ContainerData dragItem1 = new();
    private MattelClass.SheetData dragItem2 = new();
    private MattelClass.LayoutList layoutList = new();
    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.SheetList sheetList = new();
    private MattelClass.LayoutData layoutData = new();
    private MattelClass.OrderKit orderKit = new();
    private MattelClass.ContainerTypeList containerTypeList = new();
    private MattelClass.Sheet partNumbers = new();
    private PopMsgData popMsgData = new();
    private List<MattelClass.Sheet> filteredKitNumbers = new();
    private List<MattelClass.LayoutData> filteredLayoutNames = new();
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
    ~SetupOrderKit()
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


            var tempList1 = MattelService.GetSheetList();
            var tempList2 = new MattelClass.SheetList();

            orderKitList = MattelService.GetOrderKitList();

            if (ID <= 0)
            {
                foreach (var item in tempList1.Sheet)
                {
                    var findData = orderKitList.OrderKit.FirstOrDefault(
                        n => n.KitNumber == item.KitNumber
                    );

                    if (findData == null)
                        tempList2.Sheet.Add(item);
                }
            }
            else if (ID > 0)
            {
                foreach (var item in tempList1.Sheet)
                {
                    var findData = orderKitList.OrderKit.FirstOrDefault(
                        n => n.KitNumber == item.KitNumber && n.ID != ID
                    );

                    if (findData == null)
                        tempList2.Sheet.Add(item);
                }

                orderKit = orderKitList.OrderKit.FirstOrDefault(
                    n => n.ID == ID
                )!;
            }

            sheetList.Sheet = tempList2.Sheet.OrderBy(
                n => n.KitNumber
            ).ToList();

            containerTypeList = MattelService.GetContainerTypeList();
            dragItem1 = containerTypeList.List[0];
            isEye = true;
            isOrderKit = true;
            isLiveRun = false;

            if (orderKit != null)
            {
                partNumbers = sheetList.Sheet.FirstOrDefault(
                    n => n.KitNumber == orderKit.KitNumber
                )!;

                var tempList3 = MattelService.GetLayoutList();
                layoutList.List = tempList3.List.OrderBy(
                    n => n.LayoutName
                ).ToList();
                layoutList = MattelService.GetLayoutList();

                layoutData = layoutList.List.FirstOrDefault(
                    n => n.LayoutName == orderKit.LayoutName
                )!;
            }
            else
            {
                layoutData = null!;
            }

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

            filteredKitNumbers = sheetList.Sheet;
            filteredLayoutNames = layoutList.List;
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

    private async void FilterLayout(string filter)
    {
        showLayoutDropdown = true;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredLayoutNames = layoutList.List
                .Where(u => u.LayoutName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            selectFontColor = "color: rgba(0, 0, 0, 1)";
        }
        else
        {
            filteredLayoutNames = layoutList.List;
            selectFontColor = "color: rgba(0, 0, 0, 0.6)";
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void SelectKitNumber(string kitNumber)
    {
        orderKit.KitNumber = kitNumber;
        showKitDropdown = false;
        HandleOnChange1(kitNumber);
        await InvokeAsync(StateHasChanged);
    }

    private async void SelectLayoutName(string layoutName)
    {
        orderKit.LayoutName = layoutName;
        showLayoutDropdown = false;
        HandleOnChange2(layoutName);
        await InvokeAsync(StateHasChanged);
    }

    private async void ToggleKitDropdown()
    {
        showKitDropdown = !showKitDropdown; ;
        showLayoutDropdown = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void ToggleLayoutDropdown()
    {
        showLayoutDropdown = !showLayoutDropdown;
        showKitDropdown = false;
        await InvokeAsync(StateHasChanged);
    }

    private void HandleOnChange1(string SelectedValue)
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
            layoutData = layoutList.List.FirstOrDefault(
                n => n.LayoutName == SelectedValue
            )!;

            orderKit.OrderKitData = new();

            if (layoutData != null)
            {
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
                    layoutData!.LayoutTypeName,
                    layoutData,
                    dragItem1,
                    isEye,
                    isOrderKit,
                    dragItem2,
                    orderKit,
                    isLiveRun
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

    private void OnValidSubmit(MattelClass.OrderKit OrderKit)
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

            if (param2 is MattelClass.OrderKit)
            {
                orderKit = param2;
                IsCreateButtonEnabled = AreAllSKUsAssigned();
                var findOrderKit = orderKitList.OrderKit.FirstOrDefault(
                    n => n.OrderKitNumber == orderKit.OrderKitNumber
                );

                if (findOrderKit != null)
                {
                    findOrderKit = orderKit;
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

    private async void HandleDragStart(MattelClass.SheetData SheetData)
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

    private void HandleDrop(ref MattelClass.SheetData SheetData)
    {
        SheetData = dragItem2;
        StateHasChanged();
    }

    private void HandleDragLeave()
    {

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
                $"OnClickAdd ID={ID}, layoutData=",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"{CommonLib.JsonSerialize(orderKit)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (ID == -1 && orderKit != null)
            {
                if (!MattelService.IsDuplicateOrderKitNumber(orderKit))
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"OnClickAdd data added",
                        settingLevel,
                        MessageLevel.Trace
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
                else
                {
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Layout name \"{orderKit.OrderKitNumber}\"";
                    popMsgData.Text_2 = "already exist";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
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
                $"{CommonLib.JsonSerialize(orderKit)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (ID > 0 && orderKit != null)
            {
                var index = orderKitList.OrderKit.FindIndex(
                    n => n.ID == orderKit.ID
                );

                if (index != -1)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"OnClickSave data saved",
                        settingLevel,
                        MessageLevel.Information
                    );

                    orderKitList.OrderKit[index] = orderKit;
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = "Kit Layout updated";
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
                    popMsgData.Text_1 = $"Layout name \"{orderKit.OrderKitNumber}\"";
                    popMsgData.Text_2 = "data error";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = () => OnClickOk();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
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
            foreach (var item in orderKit.OrderKitData)
            {
                item.PartNumber = string.Empty;
                item.Description = string.Empty;
            }

            layoutData = layoutList.List.FirstOrDefault(
                n => n.LayoutName == orderKit.LayoutName
            )!;

            IsCreateButtonEnabled = AreAllSKUsAssigned();
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
            orderKit.OrderKitNumber = orderKit.KitNumber;
            orderKit.CreatedData = DateTime.Now;
            MattelService.AddOrderKit(orderKit);
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
            orderKit.OrderKitNumber = orderKit.KitNumber;
            orderKit.CreatedData = DateTime.Now;
            MattelService.EditOrderKit(orderKit);
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
}
using BarcodeStandard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class LineView : IDisposable
{
    [Parameter]
    public dynamic? ParentVariable1 { get; set; }

    [Parameter]
    public dynamic? ParentVariable2 { get; set; }

    [Parameter]
    public EventCallback<dynamic> ParentVariableChanged { get; set; }

    private string taskName = "LineView";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LineView;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private MattelClass.ProductionList productionList = new();
    private MattelClass.LayoutList layoutList = new();
    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.LayoutData layoutData = new();
    private MattelClass.OrderKit orderKit = new();
    private MattelClass.ContainerData dragItem1 = new();
    private MattelClass.SheetData dragItem2 = new();
    private List<MattelClass.OrderKitData> partnumbers = null!;
    private bool isEye = false;
    private bool isOrderKit = false;
    private bool isLiveRun = false;
    private string imageSrc = string.Empty;

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
        // if (FirstRender && urlPage == taskName)
        if (FirstRender)
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
    ~LineView()
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

            // if (urlPage != taskName)
            //     return Task.CompletedTask;

            if (ParentVariable2)
            {
                orderKitList = MattelService.GetOrderKitList();
                orderKit = orderKitList?.OrderKit?.FirstOrDefault(
                   n => n.ID == ParentVariable1
                )!;
            }
            else
            {
                productionList = MattelService.GetProductionList();
                var lineData = productionList.ProductionData.FirstOrDefault(
                   n => n.ID == ParentVariable1
                ) ?? null!;

                orderKitList = MattelService.GetOrderKitList();

                if (ParentVariable1 != null && lineData != null)
                {
                    orderKit = orderKitList?.OrderKit?.FirstOrDefault(
                       n => n.OrderKitNumber == lineData?.OrderKitNumber
                    )!;
                }
                else
                {
                    orderKit = null!;
                }
            }

            if (orderKit != null)
            {
                partnumbers = orderKit.OrderKitData
                    .Where(n => !string.IsNullOrEmpty(n.PartNumber))
                    .GroupBy(n => n.PartNumber)
                    .Select(n => n.First())
                    .ToList();
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

            dragItem1 = new();
            isEye = true;
            isOrderKit = true;
            dragItem2 = new();
            isLiveRun = false;

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

    private async Task RequestFromChild()
    {
        await InvokeAsync(StateHasChanged);
        return;
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
            await ParentVariableChanged.InvokeAsync(true);
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

    private async void OnClickPrint()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickOK triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (ParentVariable1 != null)
            {
                await JsInteropHelper.PrintSpecificArea(jsRuntime);
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

    public string GetBarcodeImage(
        string Text,
        int width = 200,
        int height = 30
    )
    {
        string image = string.Empty;
        var b = new Barcode();
        b.IncludeLabel = false;
        using var img = b.Encode(BarcodeStandard.Type.Code128, Text, width, height);
        using (var data = img.Encode())
        {
            var bytes = data.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            image = $"data:image/png;base64,{base64}";
        }

        return image;
    }
}
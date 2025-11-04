using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class Standard : IDisposable
{
    [Parameter]
    public dynamic? ParentVariable1 { get; set; }

    [Parameter]
    public dynamic? ParentVariable2 { get; set; }

    [Parameter]
    public dynamic? ParentVariable3 { get; set; }

    [Parameter]
    public dynamic? ParentVariable4 { get; set; }

    [Parameter]
    public dynamic? ParentVariable5 { get; set; }

    [Parameter]
    public dynamic? ParentVariable6 { get; set; }

    [Parameter]
    public dynamic? ParentVariable7 { get; set; }

    [Parameter]
    public EventCallback<(dynamic, dynamic)> ParentVariableChanged { get; set; }

    [Parameter]
    public EventCallback ChildCallback { get; set; }

    private string taskName = "Standard";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_Standard;
    private bool disposed = false;
    private IDisposable? registration;
    private int minBoxes = -1;
    private int maxBoxes = -1;
    private int count = 0;
    private int posX1 = 0;
    private int posY1 = 0;
    private int posX2 = 0;
    private int posY2 = 0;
    private int posX3 = 0;
    private int posY3 = 0;
    private string boxStyle1 = string.Empty;
    private string boxStyle2 = string.Empty;
    private string textStyle1 = string.Empty;
    private string textStyle2 = string.Empty;
    private string htmlStyle1 = string.Empty;
    private string htmlStyle2 = string.Empty;
    private string htmlStyle3 = string.Empty;
    private string hiddenStyle1 = string.Empty;
    private string hiddenStyle2 = "display: none;";
    private string hiddenStyle3 = "display: none;";
    private string hiddenStyle4 = "display: none;";
    private MattelClass.LayoutData layoutData = null!;
    private MattelClass.ContainerTypeList containerTypeList = new();
    private MattelClass.OrderKit orderKit = null!;
    private string gotTableStyle = string.Empty;
    private string gotCommandStyle = string.Empty;

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

                containerTypeList = MattelService.GetContainerTypeList();
                layoutData = ParentVariable1!;
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
    ~Standard()
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

    protected override async Task OnParametersSetAsync()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnParametersSetAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            containerTypeList = MattelService.GetContainerTypeList();
            layoutData = ParentVariable1!;
            orderKit = ParentVariable6!;

            if (ParentVariable3)
            {
                hiddenStyle1 = "display: none;";
            }
            else
            {
                hiddenStyle1 = "";
            }

            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync; ParentVariable1={CommonLib.JsonSerialize(ParentVariable1)}",
                settingLevel,
                MessageLevel.Disable
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable2={CommonLib.JsonSerialize(ParentVariable2)}",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable3={CommonLib.JsonSerialize(ParentVariable3)}",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable4={CommonLib.JsonSerialize(ParentVariable4)}",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable5={CommonLib.JsonSerialize(ParentVariable5)}",
                settingLevel,
                MessageLevel.Trace
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable6={CommonLib.JsonSerialize(ParentVariable6)}",
                settingLevel,
                MessageLevel.Disable
            );
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable7={CommonLib.JsonSerialize(ParentVariable7)}",
                settingLevel,
                MessageLevel.Trace
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

        await Task.CompletedTask;
    }

    public override async Task SetParametersAsync(ParameterView Parameters)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SetParametersAsync triggered",
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

        await base.SetParametersAsync(Parameters);
    }

    private async Task UpdateParent()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateParent triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (ParentVariableChanged.HasDelegate
                && ParentVariable1 != null
                && ParentVariable6 != null
            )
                await ParentVariableChanged.InvokeAsync((ParentVariable1!, ParentVariable6!));
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

    public async Task ChildMethod()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"ChildMethod triggered",
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

        await Task.CompletedTask;
    }

    private void HandleDragStart()
    {
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

    private void HandleDragEnter()
    {
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

    private async void HandleDrop(MattelClass.LineData LineData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleDrop triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (!ParentVariable7)
            {
                MattelClass.ContainerData containerData = ParentVariable2 ?? null!;
                MattelClass.SheetData sheetData = ParentVariable5 ?? null!;

                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop LineData={CommonLib.JsonSerialize(LineData)}",
                    settingLevel,
                    MessageLevel.Trace
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop ParentVariable2={CommonLib.JsonSerialize(ParentVariable2)}",
                    settingLevel,
                    MessageLevel.Trace
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop ParentVariable5={CommonLib.JsonSerialize(ParentVariable5)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (!ParentVariable4)
                {
                    if (containerData != null)
                    {
                        LineData.ContainerType = containerData.ContainerType;
                        LineData.BoxColor = containerData.BoxColor;
                        LineData.FontColor = containerData.FontColor;
                    }
                }
                else
                {
                    if (sheetData != null)
                    {
                        var findKitData = orderKit.OrderKitData.FirstOrDefault(
                            n => n.Point == LineData.Point
                        );

                        if (findKitData != null)
                        {
                            findKitData.PartNumber = sheetData.PartNumber;
                        }
                    }
                }

                await InvokeAsync(StateHasChanged);
                await UpdateParent();
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

    private async void HandleDrop2(MattelClass.LineData LineData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleDrop2 triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (!ParentVariable7)
            {
                MattelClass.ContainerData containerData = ParentVariable2 ?? null!;
                MattelClass.SheetData sheetData = ParentVariable5 ?? null!;

                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop2 LineData={CommonLib.JsonSerialize(LineData)}",
                    settingLevel,
                    MessageLevel.Trace
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop2 ParentVariable2={CommonLib.JsonSerialize(ParentVariable2)}",
                    settingLevel,
                    MessageLevel.Trace
                );
                CommonLib.DisplayConsole(
                    taskName,
                    $"HandleDrop2 ParentVariable5={CommonLib.JsonSerialize(ParentVariable5)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (!ParentVariable4)
                {
                    if (containerData != null)
                    {
                        LineData.TableType = containerData.ContainerType;
                        LineData.TableBoxColor = containerData.BoxColor;
                        LineData.TableFontColor = containerData.FontColor;
                    }
                }

                await InvokeAsync(StateHasChanged);
                await UpdateParent();
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

    private void HandleDragLeave()
    {
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

    private async void OnClickBox(MattelClass.LineData LineData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickBox triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (ParentVariable7 && ParentVariableChanged.HasDelegate)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"OnClickBox LineData={CommonLib.JsonSerialize(LineData)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                var findKitData = orderKit.OrderKitData.FirstOrDefault(
                    n => n.Point == LineData.Point
                );

                if (findKitData != null)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"OnClickBox findKitData={CommonLib.JsonSerialize(findKitData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    await ParentVariableChanged.InvokeAsync((LineData, findKitData));
                    await UpdateParent();
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
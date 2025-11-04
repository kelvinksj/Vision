using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CPC : IDisposable
{
    [Parameter]
    public dynamic? ParentVariable1 { get; set; }

    [Parameter]
    public dynamic? ParentVariable2 { get; set; }

    [Parameter]
    public dynamic? ParentVariable3 { get; set; }

    [Parameter]
    public EventCallback<dynamic> ParentVariableChanged { get; set; }

    [Parameter]
    public EventCallback ChildCallback { get; set; }

    private string taskName = "CPC";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_CPC;
    private bool disposed = false;
    private IDisposable? registration;
    private int posX = 0;
    private int posY = 0;
    private string htmlStyle1 = string.Empty;
    private string displayType = string.Empty;
    private List<MattelClass.LineData> lineData = [];
    private List<MattelClass.LocationSetting> locSetting = [];
    private MattelClass.LayoutAreaList layoutAreaList = new();
    private List<MattelClass.LayoutArea> layoutAreas = [];
    private bool isLineEdit = false;

    private bool disableLine => (
        !isLineEdit
    );
    private string disableStyleLine =>
    disableLine
    ? "display:none;"
    : "display:block;";

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
    ~CPC()
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
            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable1={CommonLib.JsonSerialize(ParentVariable1)}",
                settingLevel,
                MessageLevel.Trace
            );

            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync: ParentVariable2={CommonLib.JsonSerialize(ParentVariable2)}",
                settingLevel,
                MessageLevel.Trace
            );

            if (ParentVariable1 is List<MattelClass.LineData>)
            {
                lineData = ParentVariable1;
            }
            else
            {
                lineData = [];
            }

            if (ParentVariable2 is List<MattelClass.LocationSetting>)
            {
                locSetting = ParentVariable2;
            }
            else
            {
                locSetting = new();

                if (lineData?.Count > 0)
                {
                    foreach (var item in lineData)
                    {
                        locSetting.Add(
                            new()
                            {
                                LocationID = item.Point,
                                Parameter = string.Empty
                            }
                        );
                    }
                }
            }

            if (ParentVariable3 is bool)
            {
                isLineEdit = ParentVariable3;
            }
            else
            {
                isLineEdit = false;
            }

            layoutAreaList = MattelService.GetLayoutAreaList();
            layoutAreas = layoutAreaList.LayoutArea.Where(
                n => n.LayoutType == "Production"
            ).OrderBy(
                n => n.LineCode
            ).ToList();
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
            CommonLib.DisplayConsole(
                taskName,
                $"OnClickBox: Point={LineData.Point}, ContainerType={LineData.ContainerType}",
                settingLevel,
                MessageLevel.Trace
            );

            if (LineData.ContainerType == "Table"
                || LineData.ContainerType == "Loaded"
                || LineData.ContainerType == "Full"
            )
                await ParentVariableChanged.InvokeAsync(LineData);
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
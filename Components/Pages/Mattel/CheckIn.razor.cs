using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CheckIn : IDisposable
{
    [Parameter]
    public dynamic? ParentVariable1 { get; set; }

    [Parameter]
    public EventCallback<dynamic> ParentVariableChanged { get; set; }

    [Parameter]
    public EventCallback ChildCallback { get; set; }

    private string taskName = "CheckIn";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_CheckIn;
    private bool disposed = false;
    private IDisposable? registration;
    private int posX = 0;
    private int posY = 0;
    private string htmlStyle1 = string.Empty;
    private string displayType = string.Empty;
    private List<MattelClass.LineData> lineData = new();

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
    ~CheckIn()
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
            lineData = ParentVariable1!;

            CommonLib.DisplayConsole(
                taskName,
                $"OnParametersSetAsync ParentVariable1={CommonLib.JsonSerialize(ParentVariable1)}",
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
}
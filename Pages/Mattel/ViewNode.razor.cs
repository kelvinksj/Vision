using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Models.RCS;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class ViewNode : IDisposable
{
    [Parameter]
    public EventCallback<RCSModel.NodeData> UpdatePosition { get; set; }

    private string taskName = "ViewNode";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ViewPort;
    private bool disposed = false;
    private IDisposable? registration;
    private int posX = 0;
    private int posY = 0;
    private string htmlStyle1 = string.Empty;
    private string contentType = string.Empty;
    private MattelClass.LayoutAreaList layoutAreaList = new();
    private RCSModel.NodeList nodeList = new();
    private RCSModel.NodeList filterNodeList = new();
    private MattelClass.LayoutArea seletedArea = new();
    private RCSModel.NodeData selectedPosition = new();

    private bool disableBtn =>
        (
            string.IsNullOrWhiteSpace(selectedPosition.Content)
        );
    private string disableStyle =>
        (
            string.IsNullOrWhiteSpace(selectedPosition.Content)
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

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
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
                await jsRuntime.InvokeVoidAsync("initializeMap");
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
    ~ViewNode()
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
            layoutAreaList = MattelService.GetLayoutAreaList();
            seletedArea = layoutAreaList.LayoutArea.FirstOrDefault() ?? null!;
            nodeList = MattelService.GetRCSNodeList();

            if (seletedArea != null)
                try
                {
                    filterNodeList.List = nodeList.List.Where(
                        n => int.Parse(n.Content[..4]) >= seletedArea.AreaCodeStart
                        && int.Parse(n.Content[..4]) <= seletedArea.AreaCodeEnd
                    ).ToList();
                }
                catch
                {
                    filterNodeList = nodeList;
                }
            else
                filterNodeList = nodeList;

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

    private async Task HandleOnChange(int ID)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"HandleOnChange: ID={ID}",
                settingLevel,
                MessageLevel.Trace
            );

            selectedPosition = new();
            nodeList = MattelService.GetRCSNodeList();
            layoutAreaList = MattelService.GetLayoutAreaList();
            seletedArea = layoutAreaList.LayoutArea.FirstOrDefault(
                n => n.ID == ID
            ) ?? null!;

            if (seletedArea != null)
                try
                {
                    filterNodeList.List = nodeList.List.Where(
                        n => int.Parse(n.Content[..4]) >= seletedArea.AreaCodeStart
                        && int.Parse(n.Content[..4]) <= seletedArea.AreaCodeEnd
                    ).ToList();
                }
                catch
                {
                    filterNodeList = nodeList;
                }
            else
                filterNodeList = nodeList;

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

    private void OnClickBox(RCSModel.NodeData NodeData)
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
                $"OnClickBox: Content={NodeData.Content}, Name={NodeData.Name}",
                settingLevel,
                MessageLevel.Trace
            );

            selectedPosition = NodeData;
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

    public async void OnClickSubmit()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickSubmit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnClickBox: Content={selectedPosition.Content}, Name={selectedPosition.Name}",
                settingLevel,
                MessageLevel.Trace
            );

            await UpdatePosition.InvokeAsync(selectedPosition);
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
            $"OnClickCancel triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await UpdatePosition.InvokeAsync(new());
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
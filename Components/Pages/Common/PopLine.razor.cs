using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;

namespace TaskManagerWeb.Components.Pages.Common;
public partial class PopLine : IDisposable
{
    [Parameter]
    public PopLineData PopLineData { get; set; } = new();

    private static string taskName = "PopLine";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_PopLine;
    private bool disposed = false;
    public string boxType = string.Empty;
    public string boxStyle = string.Empty;
    public string textStyle = string.Empty;

    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

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
                PopLineData.BoxType = (BoxType) => SetBoxType(BoxType);
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
    ~PopLine()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    private async void SetBoxType(string BoxType)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SetBoxType triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            switch (BoxType)
            {
                case "NA":
                    boxType = "NA";
                    boxStyle = "background-color: #d3d3d3;";
                    textStyle = "color: black;";
                    break;

                case "SKU":
                    boxType = "SKU";
                    boxStyle = "background-color: #ffbd62;";
                    textStyle = "color: black;";
                    break;

                case "GL":
                    boxType = "GL";
                    boxStyle = "background-color: #874f41;";
                    textStyle = "color: white;";
                    break;

                case "FG":
                    boxType = "FG";
                    boxStyle = "background-color: #f36d33;";
                    textStyle = "color: white;";
                    break;

                default:
                    boxType = string.Empty;
                    boxStyle = string.Empty;
                    textStyle = string.Empty;
                    break;
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
}
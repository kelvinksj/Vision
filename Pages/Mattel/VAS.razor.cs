using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.RCS;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class VAS : IDisposable
{
    private string taskName = "VAS";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_VAS;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private int posX = 0;
    private int posY = 0;
    private string htmlStyle1 = string.Empty;
    private string contentType = string.Empty;
    private RCSModel.NodeList nodeList = new();
    private PopMsgData popMsgData = new();
    private string areaCode = "2101";

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
            && !authUser.IsInRole("Operator")
        )
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

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
                ContainerService.SubscribeContainer(ContainerUpdate, areaCode);
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
                    ContainerService.UnsubscribeContainer(ContainerUpdate, areaCode);
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
    ~VAS()
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
            nodeList = new();
            var nodes = MattelService.GetRCSNodeList();

            foreach (var node in nodes.List)
            {
                if (node.Content[..4].Contains(areaCode, StringComparison.OrdinalIgnoreCase))
                    nodeList.List.Add(node);
            }

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

    private async void OnClickBox(RCSModel.NodeData NodeData)
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
                $"OnClickBox: Content={NodeData.Content},"
                + $" ShowType={NodeData.ShowType}",
                settingLevel,
                MessageLevel.Trace
            );

            if (NodeData.ShowType == "Table"
                || NodeData.ShowType == "Loaded"
                || NodeData.ShowType == "Full"
            )
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Confirm to checkout";
                popMsgData.Text_2 = "this item ?";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_1 = () => OnClickOk(NodeData);
                popMsgData.BtnClick_2 = () => OnClickNok();

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

    public async void OnClickOk(RCSModel.NodeData NodeData)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCommit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
            await InvokeAsync(StateHasChanged);

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = NodeData.ContainerData.LoadType,
                    LocationID = NodeData.ContainerData.Location,
                    KitID = NodeData.ContainerData.KitId,
                    SKUID = NodeData.ContainerData.SkuId,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.CheckOut
                }
            };

            await TaskManagerService.AddCommandQueue(httpUIData);
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

    public async void OnClickNok()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickNok triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = false;
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
            if (Containers?.Count > 0)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    $"ContainerUpdate: count{Containers.Count}",
                    settingLevel,
                    MessageLevel.Trace
                );

                foreach (var item in Containers)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        $"ContainerUpdate: Location={item.Location}",
                        settingLevel,
                        MessageLevel.Disable
                    );

                    var findData = nodeList.List.FirstOrDefault(
                        n => n.Content == item.Location
                    );

                    if (findData != null)
                    {
                        findData.ContainerData = item.DeepClone();

                        if (item.Status == HttpClass.StatusType.LoadOut)
                        {
                            findData.ShowType = "LoadOut";
                        }
                        else if (item.LocCondition == HttpClass.ConditionType.CheckOut)
                        {
                            findData.ShowType = "CheckOut";
                        }
                        else if (item.ContainerId == string.Empty)
                        {
                            findData.ShowType = "Empty";
                        }
                        else if (item.Status == HttpClass.StatusType.Empty)
                        {
                            findData.ShowType = "Table";
                        }
                        else if (item.Status == HttpClass.StatusType.Loaded
                            || item.Status == HttpClass.StatusType.Full
                        )
                        {
                            findData.ShowType = "Loaded";
                        }
                        else if (item.Status == HttpClass.StatusType.LoadOut)
                        {
                            findData.ShowType = "LoadOut";
                        }
                        else
                        {
                            findData.ShowType = "NA";
                        }
                    }
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

        await Task.CompletedTask;
    }
}
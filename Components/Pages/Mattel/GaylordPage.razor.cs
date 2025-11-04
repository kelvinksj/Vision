using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class GaylordPage : IDisposable
{
    private string taskName = "GaylordPage";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_GaylordPage;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private UserAccount user = new UserAccount();
    private IDisposable? registration;
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private PopContainerData popContainerData = new();
    private MsgList mList = new();
    private string areaCode = "2401";
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

                ContainerService.SubscribeContainer(ContainerUpdate, areaCode);
                TaskManagerService?.SubscribeMessage(MsgUpdate, areaCode);
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

                    try
                    {
                        ContainerService.UnsubscribeContainer(ContainerUpdate, areaCode);
                        TaskManagerService?.UnsubscribeMessage(MsgUpdate, areaCode);
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
    ~GaylordPage()
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

            lineData = [];

            for (int j = 1; j <= 1; j++)
            {
                string area = $"240{j}";
                int id = 1;

                if (lineData.Count > 0)
                    id = lineData.Max(n => n.ID) + 1;

                for (int i = 1; i <= 12; i++)
                {
                    lineData.Add(new()
                    {
                        ID = id,
                        Point = $"{area}{i:0000}",
                        ContainerType = "Empty"
                    });

                    id++;
                }
            }

            SetDynamicComponent(
                "Gaylord",
                lineData
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
        dynamic ParentVariable1
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
                        = EventCallback.Factory.Create<dynamic>(this, value => UpdateFromChild(value));

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
                    builder.AddAttribute(2, "ParentVariableChanged", callback);
                    builder.AddAttribute(3, "ChildCallback", childCallback);
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

    private Task UpdateFromChild(dynamic Data)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateFromChild triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (Data is MattelClass.LineData)
            {
                MattelClass.LineData lineData = Data;

                popContainerData.ActivatePopup = true;
                popContainerData.IconClick = null!;
                popContainerData.Text_1 = "Confirm dumpster";
                popContainerData.Text_2 = "empty ?";
                popContainerData.Point = lineData.Point;
                popContainerData.ContainerType = lineData.ContainerType;
                popContainerData.BtnText_1 = "OK";
                popContainerData.BtnClick_1 = () => OnClickOk();
                popContainerData.BtnText_2 = "Cancel";
                popContainerData.BtnClick_2 = () => OnClickCancel();
                popContainerData.BtnText_3 = null!;
                popContainerData.BtnClick_3 = null!;
                popContainerData.BtnText_4 = null!;
                popContainerData.BtnClick_4 = null!;
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

        return Task.CompletedTask;
    }

    public async void OnClickOk()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateRequest triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"UpdateRequest: Point={popContainerData.Point}, ContainerType={popContainerData.ContainerType}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = HttpClass.OrderLoadType.Gaylord,
                    LocationID = popContainerData.Point,
                    KitID = string.Empty,
                    SKUID = string.Empty,
                    Behaviour = string.Empty,
                    CommandID = HttpClass.UICommand.Clear
                }
            };

            var result = await TaskManagerService.AddCommandQueue(httpUIData);
            popContainerData.ActivatePopup = false;
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

    public void OnClickCancel()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateClear triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            popContainerData.ActivatePopup = false;
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
                foreach (var item in Containers)
                {
                    var findData = lineData.FirstOrDefault(
                        n => n.Point == item.Location
                    );

                    if (findData != null)
                    {
                        if (item.ContainerId == string.Empty)
                            findData.ContainerType = "Empty";
                        else if (item.Status == HttpClass.StatusType.Empty)
                            findData.ContainerType = "Table";
                        else if (item.Status == HttpClass.StatusType.Loaded
                            || item.Status == HttpClass.StatusType.Full
                        )
                            findData.ContainerType = "Loaded";
                        else
                            findData.ContainerType = "NA";
                    }
                }

                SetDynamicComponent(
                    "Gaylord",
                    lineData
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

    private async void MsgUpdate(List<HttpClass.UIMessage> UIMessages)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"MsgUpdate triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            if (UIMessages != null)
            {
                var count = 1;
                mList = new();

                foreach (var msg in UIMessages)
                {
                    DisplayString title = new();
                    CommonLib.CopyProperties(msg.Title, title);
                    List<DisplayString> textList = new();

                    foreach (var text in msg.TextList)
                    {
                        DisplayString text2 = new();
                        CommonLib.CopyProperties(text, text2);
                        textList.Add(text2);
                    }

                    mList.List.Add(
                        new()
                        {
                            ID = count++,
                            TaskID = msg.TaskID,
                            Title = title,
                            TextList = textList,
                            OnClick = () => MsgCancel(msg.TaskID)
                        }
                    );
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
    }

    private void MsgCancel(string TaskID)
    {
        CommonLib.DisplayConsole(
           taskName,
           $"MsgCancel triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            TaskManagerService?.AddCancelMessageQueue(TaskID);
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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging.Abstractions;
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
using ZXing.Client.Result;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class LineEdit : IDisposable
{
    [Parameter]
    public dynamic? ParentVariable1 { get; set; }

    [Parameter]
    public dynamic? ParentVariable2 { get; set; }

    [Parameter]
    public EventCallback<dynamic> ParentVariableChanged { get; set; }

    private string taskName = "LineEdit";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_LineEdit;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private IDisposable? registration;
    private string selectFontColor = string.Empty;
    private MattelClass.ProductionList productionList = new();
    private MattelClass.OrderKitList orderKitList = new();
    private MattelClass.ProductionData lineData = new();
    private PopMsgData popMsgData = new();

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
    ~LineEdit()
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

            productionList = MattelService.GetProductionList();
            lineData = productionList.ProductionData.FirstOrDefault(
               n => n.ID == ParentVariable1
            ) ?? null!;

            if (productionList != null && lineData != null)
            {
                if (orderKitList != null)
                {
                    var tempList = MattelService.GetOrderKitList();
                    orderKitList = MattelService.GetOrderKitList();

                    foreach (var item in tempList.OrderKit)
                    {
                        if (item.OrderKitNumber != lineData.OrderKitNumber
                            && (item.Status == "Open"
                                || item.Status == "Production"
                                || item.Status == "Completed"
                            )
                        )
                        {
                            var findOrderKit = orderKitList.OrderKit.FirstOrDefault(
                                n => n.ID == item.ID
                            );

                            if (findOrderKit != null)
                                orderKitList.OrderKit.Remove(findOrderKit);
                        }
                    }

                    foreach (var item in productionList.ProductionData)
                    {
                        if (item.OrderKitNumber != string.Empty)
                        {
                            var findOrderKit = orderKitList.OrderKit.FirstOrDefault(
                                n => n.OrderKitNumber == item.OrderKitNumber
                                    && n.OrderKitNumber != lineData.OrderKitNumber
                            );

                            if (findOrderKit != null)
                                orderKitList.OrderKit.Remove(findOrderKit);
                        }
                    }
                }
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

    private void OnClickConfirm()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickConfirm triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = "Line updated";
            popMsgData.Text_2 = "successfully !";
            popMsgData.Middle_Icon = "icon_worker";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnText_2 = null!;
            popMsgData.BtnClick_1 = () => OnClickOkConfirm();
            popMsgData.BtnClick_2 = null!;
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

    private async void OnClickOkConfirm()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickConfirm triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lineData.Status = "Open";
            lineData.CreatedData = DateTime.Now;
            await ParentVariableChanged.InvokeAsync(lineData);
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

    private async void OnClickCancel()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCancel triggered",
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

    private void OnValidSubmit()
    {

    }
}
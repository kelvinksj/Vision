using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class CheckInPage : IDisposable
{
    private readonly Guid pageGuid = Guid.NewGuid();

    private static string taskName = "CheckInPage";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_CheckInPage;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private static IList<UserAccount> users = [];
    private UserAccount user = new();
    private IDisposable? registration;
    private static CommonLib? commonLib;
    private string workerNo = string.Empty;
    private string? currentLayoutName = string.Empty;
    private ClaimsPrincipal authSetup = new();
    private RenderFragment? dynamicComponent;
    private EventCallback childCallback;
    private readonly static object lockCheckInList = new();
    private static MattelClass.CheckInList checkInList = new();
    private readonly static object lockLineData = new();
    private static List<MattelClass.LineData> lineData = new();
    private MattelClass.CheckInData selectedData = null!;
    private PopMsgData popMsgData = new();
    private PopContainerData popContainerData = new();
    private MsgList mList = new();
    private static string areaCode = "2001";
    private readonly static List<KeyboardData> keyboards = [];
    private string modalContent = "";
    private bool showModal = false;
    private List<string> dropdownOptions = new();

    private bool disableCancel =>
        (
            selectedData == null
        );
    private string disableStyleCancel =>
        disableCancel
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableCancelAll =>
        (
            checkInList?.List?.Count == 0
        );
    private string disableStyleCancelAll =>
        disableCancelAll
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    private bool disableManualLocation =>
        (
            (
                keyboards.FirstOrDefault(
                    n => n.Guid == pageGuid.ToString()
                )?.Current.KeyAlias ?? false
            )
            || (
                keyboards.FirstOrDefault(
                    n => n.Guid == pageGuid.ToString()
                )?.Current.KeyKit ?? false
            )
            || (
                keyboards.FirstOrDefault(
                    n => n.Guid == pageGuid.ToString()
                )?.Current.KeyPart ?? false
            )
            || (
                keyboards.FirstOrDefault(
                    n => n.Guid == pageGuid.ToString()
                )?.Current.KeyLocation ?? false
            )
        );
    private string disableStyleManualLocation =>
        disableManualLocation
        ? "background-color: #a5a8a9; border-color: #a5a8a9;"
        : string.Empty;

    public class ExtendedKeyboardEventArgs : KeyboardEventArgs
    {
        public string Guid { get; set; } = string.Empty;
    }

    private class Keyboard
    {
        public string CaptureKit { get; set; } = string.Empty;
        public string CapturePart { get; set; } = string.Empty;
        public string CaptureLocation { get; set; } = string.Empty;
        public bool KeyAlias { get; set; } = false;
        public bool KeyKit { get; set; } = false;
        public bool KeyPart { get; set; } = false;
        public bool KeyLocation { get; set; } = false;
        public bool DoneKit { get; set; } = false;
        public bool DonePart { get; set; } = false;
        public bool DoneLocation { get; set; } = false;
        public int RegisterID { get; set; } = 0;
        public string Guid { get; set; } = string.Empty;
    }

    private class KeyboardData
    {
        public string Guid { get; set; } = string.Empty;
        public bool IsPopup { get; set; } = false;
        public bool IsTriggerEnter { get; set; } = false;
        public CheckInPage? instance;
        public Keyboard Current { get; set; } = new();
        public Keyboard Old { get; set; } = new();
    }

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
                TaskManagerService.SubscribeMessage(MsgUpdate, areaCode);

                commonLib = CommonLib;

                Keyboard keyboard = new()
                {
                    CaptureKit = string.Empty,
                    CapturePart = string.Empty,
                    CaptureLocation = string.Empty,
                    KeyAlias = false,
                    KeyKit = false,
                    KeyPart = false,
                    KeyLocation = false,
                    DoneKit = false,
                    DonePart = false,
                    DoneLocation = false,
                    RegisterID = 0,
                    Guid = pageGuid.ToString()
                };

                KeyboardData keyboardData = new()
                {
                    Guid = pageGuid.ToString(),
                    IsPopup = false,
                    instance = this,
                    Current = keyboard,
                    Old = keyboard
                };

                keyboards.Add(keyboardData);
                CommonLib.DisplayConsole(
                    taskName,
                    $"OnAfterRenderAsync: remove: Guid={keyboard.Guid}, "
                    + $"count={keyboards?.Count}",
                    settingLevel,
                    MessageLevel.Trace
                );

                try
                {
                    await jsRuntime.InvokeVoidAsync(
                        "JsFunctions.addKeyboardListenerEvent",
                        "JsKeyDown",
                        pageGuid.ToString()
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

                CommonLib.DisplayConsole(
                    taskName,
                    $"OnAfterRenderAsync ativate keyboard",
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

                        try
                        {
                            await jsRuntime.InvokeVoidAsync(
                                "JsFunctions.removeKeyboardListernerEvent"
                            );
                        }
                        catch { }

                        var keyboard = keyboards.FirstOrDefault(
                            n => n.Guid == pageGuid.ToString()
                        );

                        if (keyboard != null)
                        {
                            lock (lockCheckInList)
                            {
                                var findData = checkInList.List.FirstOrDefault(
                                    n => n.ID == keyboard.Current.RegisterID
                                );

                                if (findData != null)
                                    checkInList.List.Remove(findData);
                            }

                            keyboards?.Remove(keyboard);
                            CommonLib.DisplayConsole(
                                taskName,
                                $"UpdateInfoChange: remove: Guid={keyboard.Guid}, "
                                + $"count={keyboards?.Count}",
                                settingLevel,
                                MessageLevel.Trace
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
    ~CheckInPage()
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

            lineData = new()
            {
                new(){
                    ID = 1,
                    Point = $"{areaCode}0001",
                    ContainerType = "Empty",
                },
                new(){
                    ID = 2,
                    Point = $"{areaCode}0002",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 3,
                    Point = $"{areaCode}0003",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 4,
                    Point = $"{areaCode}0004",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 5,
                    Point = $"{areaCode}0005",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 6,
                    Point = $"{areaCode}0006",
                    ContainerType = "Empty"
                },
                new(){
                    ID = 7,
                    Point = $"{areaCode}0007",
                    ContainerType = "Empty"
                }
            };

            // Initialize dropdown options
            dropdownOptions = lineData.Select(
                ld => ld.Point
            ).ToList();

            // Remove items from dropdownOptions that are already in checkInList, only if there are items in checkInList
            if (checkInList?.List?.Count > 0)
            {
                foreach (var item in checkInList.List)
                {
                    dropdownOptions.Remove(item.Point);
                }
            }

            // Ensure the dropdownOptions list is sorted
            dropdownOptions = dropdownOptions.OrderBy(loc => loc).ToList();

            SetDynamicComponent(
                "CheckIn",
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

    [JSInvokable]
    public static Task JsKeyDown(ExtendedKeyboardEventArgs e)
    {
        var keyboard = keyboards.FirstOrDefault(
            n => n.Guid == e.Guid
        );

        if (keyboard == null)
        {
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown: keyboard=",
                settingLevel,
                MessageLevel.Error
            );
            return Task.CompletedTask;
        }

        if (keyboard.instance == null)
        {
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown: instance=",
                settingLevel,
                MessageLevel.Error
            );
            return Task.CompletedTask;
        }

        if (keyboard.IsPopup)
        {
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown: IsPopup={keyboard.IsPopup}, Guid={e.Guid}",
                settingLevel,
                MessageLevel.Trace
            );
            return Task.CompletedTask;
        }

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown: Key={e.Key}, Code={e.Code}, Guid={e.Guid}",
                settingLevel,
                MessageLevel.Trace
            );

            PopMsgData popMsgData = new();

            if (e.Key == "@"
                && !keyboard.Current.KeyAlias
                && !keyboard.Current.KeyKit
                && !keyboard.Current.KeyPart
                && !keyboard.Current.KeyLocation
            )
            {
                keyboard.Current.KeyAlias = true;
                keyboard.instance.TriggerStateHasChanged();
            }
            else if (keyboard.Current.KeyAlias
                && !keyboard.Current.KeyKit
                && !keyboard.Current.KeyPart
                && !keyboard.Current.KeyLocation
            )
            {
                if (e.Key == "K")
                {
                    keyboard.Current.KeyKit = true;
                    keyboard.Current.DoneKit = false;
                    keyboard.Current.CaptureKit = string.Empty;
                    keyboard.instance.TriggerStateHasChanged();
                }
                else if (e.Key == "P")
                {
                    keyboard.Current.KeyPart = true;
                    keyboard.Current.DonePart = false;
                    keyboard.Current.CapturePart = string.Empty;
                    keyboard.instance.TriggerStateHasChanged();
                }
                else if (e.Key == "L")
                {
                    keyboard.Current.KeyLocation = true;
                    keyboard.Current.DoneLocation = false;
                    keyboard.Current.CaptureLocation = string.Empty;
                    keyboard.instance.TriggerStateHasChanged();
                }
                else if (e.Key != "Shift")
                {
                    keyboard.Current.KeyAlias = false;
                    keyboard.instance.TriggerStateHasChanged();
                }
            }

            _ = int.TryParse(e.Code, out int keyInteger);

            if ((keyInteger >= 32 && keyInteger <= 127 && e.Key != "@") || e.Key == "-")
            {
                if (keyboard.Current.KeyAlias && keyboard.Current.KeyKit)
                {
                    keyboard.Current.CaptureKit += e.Key;
                }
                else if (keyboard.Current.KeyAlias && keyboard.Current.KeyPart)
                {
                    keyboard.Current.CapturePart += e.Key;
                }
                else if (keyboard.Current.KeyAlias && keyboard.Current.KeyLocation)
                {
                    keyboard.Current.CaptureLocation += e.Key;
                }
            }

            if (
                (e.Key == "Enter"
                    | keyboard.IsTriggerEnter
                )
                && keyboard.Current.KeyAlias
                && (keyboard.Current.KeyKit
                    || keyboard.Current.KeyPart
                    || keyboard.Current.KeyLocation
                )
            )
            {
                string padString = String.Empty;

                if (keyboard.Current.KeyKit)
                {
                    padString = $"{keyboard.Current.CaptureKit[1..]}";
                    keyboard.Current.CaptureKit = padString;
                    keyboard.Current.DoneKit = true;
                }
                else if (keyboard.Current.KeyPart)
                {
                    padString = $"{keyboard.Current.CapturePart[1..]}";
                    keyboard.Current.CapturePart = padString;
                    keyboard.Current.DonePart = true;
                }
                else if (keyboard.Current.KeyLocation
                    && !keyboard.IsTriggerEnter
                )
                {
                    padString = $"{keyboard.Current.CaptureLocation[1..]}";
                    keyboard.Current.CaptureLocation = padString;
                    keyboard.Current.DoneLocation = true;
                }

                keyboard.IsTriggerEnter = false;

                if (keyboard.Current.DoneKit
                    || keyboard.Current.DonePart
                    || keyboard.Current.DoneLocation
                )
                {
                    commonLib?.DisplayConsole(
                       taskName,
                       $"JsKeyDown: Enter",
                       settingLevel,
                       MessageLevel.Trace
                    );

                    if (checkInList != null)
                    {
                        if (checkInList.List.Count <= 7)
                        {
                            bool found = false;

                            if (keyboard.Current.DoneLocation)
                            {
                                string loc = areaCode + keyboard.Current
                                                .CaptureLocation.PadLeft(4, '0')
                                                ?? string.Empty;

                                if (int.TryParse(keyboard.Current.CaptureLocation, out int number)
                                    && number > 0
                                    && number < 8
                                )
                                {
                                    var checkData =
                                        (keyboard.Current.RegisterID == 0)
                                        ? checkInList.List.FirstOrDefault(
                                            n => n.Point == loc
                                        )
                                        : checkInList.List.FirstOrDefault(
                                            n => n.Point == loc
                                            && n.ID != keyboard.Current.RegisterID
                                        );

                                    if (checkData != null)
                                    {
                                        found = true;
                                        popMsgData.Text_1 = $"location \"{loc}\"";
                                        popMsgData.Text_2 = "already scan";
                                        popMsgData.Middle_Icon = "icon_issue";
                                        popMsgData.BtnText_1 = "OK";
                                        keyboard.IsPopup = true;
                                        keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                        keyboard.Current = keyboard.Old.DeepClone();

                                        commonLib?.DisplayConsole(
                                            taskName,
                                            $"JsKeyDown location {loc} already scan",
                                            settingLevel,
                                            MessageLevel.Trace
                                        );
                                    }
                                    else
                                    {
                                        var storeContainer = lineData.FirstOrDefault(
                                            n => n.Point == loc
                                        );

                                        if (storeContainer != null)
                                        {
                                            if (storeContainer.ContainerType == "CheckOut")
                                            {
                                                found = true;
                                                popMsgData.Text_1 = $"location \"{loc}\" already";
                                                popMsgData.Text_2 = "use for check out";
                                                popMsgData.Middle_Icon = "icon_issue";
                                                popMsgData.BtnText_1 = "OK";
                                                keyboard.IsPopup = true;
                                                keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                                keyboard.Current = keyboard.Old.DeepClone();

                                                commonLib?.DisplayConsole(
                                                    taskName,
                                                    $"JsKeyDown location {loc} already use for check out",
                                                    settingLevel,
                                                    MessageLevel.Trace
                                                );
                                            }
                                            else if (storeContainer.ContainerType == "LoadOut")
                                            {
                                                found = true;
                                                popMsgData.Text_1 = $"location \"{loc}\" already";
                                                popMsgData.Text_2 = "use for load out";
                                                popMsgData.Middle_Icon = "icon_issue";
                                                popMsgData.BtnText_1 = "OK";
                                                keyboard.IsPopup = true;
                                                keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                                keyboard.Current = keyboard.Old.DeepClone();

                                                commonLib?.DisplayConsole(
                                                    taskName,
                                                    $"JsKeyDown location {loc} already use for load out",
                                                    settingLevel,
                                                    MessageLevel.Trace
                                                );
                                            }
                                            else if (storeContainer.ContainerType == "Empty")
                                            {
                                                found = true;
                                                popMsgData.Text_1 = $"location \"{loc}\" don't";
                                                popMsgData.Text_2 = "have empty table";
                                                popMsgData.Middle_Icon = "icon_issue";
                                                popMsgData.BtnText_1 = "OK";
                                                keyboard.IsPopup = true;
                                                keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                                keyboard.Current = keyboard.Old.DeepClone();

                                                commonLib?.DisplayConsole(
                                                    taskName,
                                                    $"JsKeyDown location {loc} don't have empty table",
                                                    settingLevel,
                                                    MessageLevel.Trace
                                                );
                                            }
                                            else if (storeContainer.ContainerType == "Loaded"
                                                || storeContainer.ContainerType == "Full"
                                            )
                                            {
                                                found = true;
                                                popMsgData.Text_1 = $"location \"{loc}\"";
                                                popMsgData.Text_2 = "already loaded";
                                                popMsgData.Middle_Icon = "icon_issue";
                                                popMsgData.BtnText_1 = "OK";
                                                keyboard.IsPopup = true;
                                                keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                                keyboard.Current = keyboard.Old.DeepClone();

                                                commonLib?.DisplayConsole(
                                                    taskName,
                                                    $"JsKeyDown location {loc} already loaded",
                                                    settingLevel,
                                                    MessageLevel.Trace
                                                );
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    popMsgData.Text_1 = $"location \"{loc}\"";
                                    popMsgData.Text_2 = "is invalid";
                                    popMsgData.Middle_Icon = "icon_issue";
                                    popMsgData.BtnText_1 = "OK";
                                    keyboard.IsPopup = true;
                                    keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                    keyboard.Current = keyboard.Old.DeepClone();

                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                if (keyboard.Current.RegisterID == 0)
                                {
                                    if (checkInList.List.Count < 7)
                                    {
                                        var getID = 1;

                                        lock (lockCheckInList)
                                        {
                                            if (checkInList.List.Count > 0)
                                                getID = checkInList.List.Max(n => n.ID) + 1;

                                            checkInList.List.Add(new()
                                            {
                                                ID = getID,
                                                Point = (keyboard.Current.CaptureLocation != string.Empty)
                                                    ? areaCode + keyboard.Current.CaptureLocation.PadLeft(4, '0')
                                                    : string.Empty,
                                                KitNumber = (keyboard.Current.CaptureKit != string.Empty)
                                                    ? keyboard.Current.CaptureKit
                                                    : string.Empty,
                                                PartNumber = (keyboard.Current.CapturePart != string.Empty)
                                                    ? keyboard.Current.CapturePart
                                                    : string.Empty
                                            });

                                            keyboard.Current.RegisterID = getID;
                                            keyboard.Old = keyboard.Current.DeepClone();
                                        }
                                    }
                                    else
                                    {
                                        popMsgData.Text_1 = "Already scan more";
                                        popMsgData.Text_2 = "than 7 locations";
                                        popMsgData.Middle_Icon = "icon_issue";
                                        popMsgData.BtnText_1 = "OK";
                                        keyboard.IsPopup = true;
                                        keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                                        keyboard.Current = keyboard.Old.DeepClone();
                                    }
                                }
                                else
                                {
                                    lock (lockCheckInList)
                                    {
                                        var findData = checkInList.List.FirstOrDefault(
                                            n => n.ID == keyboard.Current.RegisterID
                                        );
                                        keyboard.Old = keyboard.Current.DeepClone();

                                        if (findData != null)
                                        {
                                            findData.Point = (keyboard.Current.CaptureLocation != string.Empty)
                                                ? areaCode + keyboard.Current.CaptureLocation.PadLeft(4, '0')
                                                : string.Empty;
                                            findData.KitNumber = (keyboard.Current.CaptureKit != string.Empty)
                                                ? keyboard.Current.CaptureKit
                                                : string.Empty;
                                            findData.PartNumber = (keyboard.Current.CapturePart != string.Empty)
                                                ? keyboard.Current.CapturePart
                                                : string.Empty;
                                        }
                                    }
                                }

                                commonLib?.DisplayConsole(
                                    taskName,
                                    $"JsKeyDown update data",
                                    settingLevel,
                                    MessageLevel.Trace
                                );
                            }
                        }
                        else
                        {
                            popMsgData.Text_1 = "Already scan more";
                            popMsgData.Text_2 = "than 7 locations";
                            popMsgData.Middle_Icon = "icon_issue";
                            popMsgData.BtnText_1 = "OK";
                            keyboard.IsPopup = true;
                            keyboard.instance.TriggerPopMsg(popMsgData, keyboard.Guid);
                        }
                    }

                    if (keyboard.Current.DoneKit
                        && keyboard.Current.DonePart
                        && keyboard.Current.DoneLocation)
                    {
                        keyboard.Current.CaptureKit = string.Empty;
                        keyboard.Current.CapturePart = string.Empty;
                        keyboard.Current.CaptureLocation = string.Empty;
                        keyboard.Current.DoneKit = false;
                        keyboard.Current.DonePart = false;
                        keyboard.Current.DoneLocation = false;
                        keyboard.Current.RegisterID = 0;
                    }
                }

                keyboard.Current.KeyAlias = false;
                keyboard.Current.KeyKit = false;
                keyboard.Current.KeyPart = false;
                keyboard.Current.KeyLocation = false;
                keyboard.instance.TriggerStateHasChanged();
            }

            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown guid={keyboard.Guid}: ----------------",
                settingLevel,
                MessageLevel.Trace
            );
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown keyAlias={keyboard.Current.KeyAlias}, "
                + $"keyKit={keyboard.Current.KeyKit}, "
                + $"keyPart={keyboard.Current.KeyPart}, "
                + $"keyLocation={keyboard.Current.KeyLocation}",
                settingLevel,
                MessageLevel.Trace
            );
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown doneKit={keyboard.Current.DoneKit}, "
                + $"donePart={keyboard.Current.DonePart}, "
                + $"doneLocation={keyboard.Current.DoneLocation}",
                settingLevel,
                MessageLevel.Trace
            );
            commonLib?.DisplayConsole(
                taskName,
                $"JsKeyDown registerID={keyboard.Current.RegisterID}, "
                + $"captureKit={keyboard.Current.CaptureKit}, "
                + $"capturePart={keyboard.Current.CapturePart}, "
                + $"captureLocation={keyboard.Current.CaptureLocation}",
                settingLevel,
                MessageLevel.Trace
            );
        }
        catch (Exception ex)
        {
            commonLib?.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private void TriggerPopMsg(PopMsgData PopMsgData, string Guid)
    {
        if (Guid == pageGuid.ToString())
        {
            popMsgData = PopMsgData.DeepClone();
            popMsgData.ActivatePopup = true;
            popMsgData.BtnClick_1 = () => OnClickNok();
        }
    }

    private void SetPopStatus(bool IsPopup)
    {
        var keyboard = keyboards.FirstOrDefault(
            n => n.Guid == pageGuid.ToString()
        );

        if (keyboard != null)
        {
            keyboard.IsPopup = IsPopup;
        }
    }

    private async void TriggerStateHasChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private void ClearKeyboard(int Id)
    {
        var keyboard = keyboards.FirstOrDefault(
            n => n.Current.RegisterID == Id
        );

        if (keyboard != null)
        {
            keyboard.Current.CaptureKit = string.Empty;
            keyboard.Current.CapturePart = string.Empty;
            keyboard.Current.CaptureLocation = string.Empty;
            keyboard.Current.DoneKit = false;
            keyboard.Current.DonePart = false;
            keyboard.Current.DoneLocation = false;
            keyboard.Current.RegisterID = 0;
            keyboard.Current.KeyAlias = false;
            keyboard.Current.KeyKit = false;
            keyboard.Current.KeyPart = false;
            keyboard.Current.KeyLocation = false;
            keyboard.Old = keyboard.Current.DeepClone();
        }
    }

    private void CleaAllKeyboard()
    {
        foreach (var keyboard in keyboards)
        {
            CommonLib.DisplayConsole(
               taskName,
               $"CleaAllKeyboard: Guid={keyboard.Guid}, RegisterID={keyboard.Current.RegisterID}",
               settingLevel,
               MessageLevel.Trace
            );

            keyboard.Current.CaptureKit = string.Empty;
            keyboard.Current.CapturePart = string.Empty;
            keyboard.Current.CaptureLocation = string.Empty;
            keyboard.Current.DoneKit = false;
            keyboard.Current.DonePart = false;
            keyboard.Current.DoneLocation = false;
            keyboard.Current.RegisterID = 0;
            keyboard.Current.KeyAlias = false;
            keyboard.Current.KeyKit = false;
            keyboard.Current.KeyPart = false;
            keyboard.Current.KeyLocation = false;
            keyboard.IsPopup = false;
            keyboard.Old = keyboard.Current.DeepClone();
        }

        CommonLib.DisplayConsole(
           taskName,
           $"CleaAllKeyboard: Count={keyboards.Count}",
           settingLevel,
           MessageLevel.Trace
        );
    }

    private void SetDynamicComponent(
        string ComponentName,
        dynamic ParentVariable1
    )
    {
        try
        {
            var componentType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name.Equals(ComponentName, StringComparison.OrdinalIgnoreCase));

            if (componentType != null)
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

                dynamicComponent = builder =>
                {
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

    private async Task UpdateFromChild(dynamic Data)
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


                if (lineData.ContainerType == "LoadOut")
                {
                    popContainerData.ActivatePopup = true;
                    popContainerData.IconClick = null!;
                    popContainerData.Text_1 = "Please click to confirm";
                    popContainerData.Text_2 = "clear position ?";
                    popContainerData.Point = lineData.Point;
                    popContainerData.ContainerType = lineData.ContainerType;
                    popContainerData.BtnText_1 = "Ok";
                    popContainerData.BtnClick_1 = () => OnClickConfirm();
                    popContainerData.BtnText_2 = "Cancel";
                    popContainerData.BtnClick_2 = () => OnClickCancel2();
                    popContainerData.BtnText_3 = null!;
                    popContainerData.BtnClick_3 = null!;
                    popContainerData.BtnText_4 = null!;
                    popContainerData.BtnClick_4 = null!;
                    await InvokeAsync(StateHasChanged);
                }
                else if (lineData.ContainerType == "Empty")
                {
                    popContainerData.ActivatePopup = true;
                    popContainerData.IconClick = null!;
                    popContainerData.Text_1 = "Please click to";
                    popContainerData.Text_2 = "\"Replenish\" ?";
                    popContainerData.Point = lineData.Point;
                    popContainerData.ContainerType = lineData.ContainerType;
                    popContainerData.BtnText_1 = "Ok";
                    popContainerData.BtnClick_1 = () => OnClickReplenish();
                    popContainerData.BtnText_2 = "Cancel";
                    popContainerData.BtnClick_2 = () => OnClickCancel2();
                    popContainerData.BtnText_3 = null!;
                    popContainerData.BtnClick_3 = null!;
                    popContainerData.BtnText_4 = null!;
                    popContainerData.BtnClick_4 = null!;
                    await InvokeAsync(StateHasChanged);
                }
                else if (lineData.ContainerType == "Table")
                {
                    popContainerData.ActivatePopup = true;
                    popContainerData.IconClick = null!;
                    popContainerData.Text_1 = "Please click to";
                    popContainerData.Text_2 = "\"Clear\" ?";
                    popContainerData.Point = lineData.Point;
                    popContainerData.ContainerType = lineData.ContainerType;
                    popContainerData.BtnText_1 = "Ok";
                    popContainerData.BtnClick_1 = () => OnClickClear();
                    popContainerData.BtnText_2 = "Cancel";
                    popContainerData.BtnClick_2 = () => OnClickCancel2();
                    popContainerData.BtnText_3 = null!;
                    popContainerData.BtnClick_3 = null!;
                    popContainerData.BtnText_4 = null!;
                    popContainerData.BtnClick_4 = null!;
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

        await Task.CompletedTask;
    }

    public async void OnClickConfirm()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickConfirm triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"OnClickConfirm: Point={popContainerData.Point}, ContainerType={popContainerData.ContainerType}",
               settingLevel,
               MessageLevel.Trace
            );

            Element.Table table = new()
            {
                Location = popContainerData.Point,
                LocCondition = Element.TableState.LoadOut
            };

            _ = ContainerService.AddContainerCommand(table);
            popContainerData.ActivatePopup = false;
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

    public async void OnClickReplenish()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickReplenish triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"OnClickReplenish: Point={popContainerData.Point}, ContainerType={popContainerData.ContainerType}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = HttpClass.OrderLoadType.SKU,
                    LocationID = popContainerData.Point,
                    KitID = "NA",
                    SKUID = "NA",
                    Behaviour = "Any",
                    CommandID = HttpClass.UICommand.Request
                }
            };

            await TaskManagerService.AddCommandQueue(httpUIData);
            popContainerData.ActivatePopup = false;
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

    public async void OnClickClear()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickClear triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            CommonLib.DisplayConsole(
               taskName,
               $"OnClickClear: Point={popContainerData.Point}, ContainerType={popContainerData.ContainerType}",
               settingLevel,
               MessageLevel.Trace
            );

            HttpClass.HttpUICommand httpUIData = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Command = new()
                {
                    LoadType = HttpClass.OrderLoadType.SKU,
                    LocationID = popContainerData.Point,
                    KitID = "NA",
                    SKUID = "NA",
                    Behaviour = "Any",
                    CommandID = HttpClass.UICommand.Clear
                }
            };

            await TaskManagerService.AddCommandQueue(httpUIData);
            popContainerData.ActivatePopup = false;
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

    public async void OnClickCancel2()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCancel2 triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            popContainerData.ActivatePopup = false;
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

    private void SelectRow(MattelClass.CheckInData CheckInData)
    {
        selectedData = CheckInData;
    }

    private string GetRow(MattelClass.CheckInData CheckInData)
    {
        return selectedData != null && selectedData.ID == CheckInData.ID
            ? "row-active"
            : string.Empty;
    }

    private string GetUserRow(MattelClass.CheckInData CheckInData)
    {
        var keyboard = keyboards.FirstOrDefault(
            n => n.Guid == pageGuid.ToString()
        );

        return keyboard != null && CheckInData.ID == keyboard.Current.RegisterID
            ? "background-color: #FFBD62; color: #6e7173;"
            : string.Empty;
    }

    private string GetUserRow2(MattelClass.CheckInData CheckInData)
    {
        var keyboard = keyboards.FirstOrDefault(
            n => n.Guid == pageGuid.ToString()
        );

        return keyboard != null && CheckInData.ID == keyboard.Current.RegisterID
            ? "color: black;"
            : string.Empty;
    }

    public async void OnClickCommit()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCommit triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (checkInList?.List?.Count > 0)
            {
                await JsInteropHelper.FocusMyHeader(jsRuntime);

                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Are you ready to";
                popMsgData.Text_2 = "Commit All ?";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_1 = () => OnClickOKCommit();
                popMsgData.BtnClick_2 = () => OnClickNok();
                SetPopStatus(true);
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

    public async void OnClickOKCommit()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickOKCommit triggered",
           settingLevel,
           MessageLevel.Information
        );

        try
        {
            lock (lockCheckInList)
            {
                lock (lockLineData)
                {
                    if (checkInList?.List?.Count > 0)
                    {
                        var commitList = checkInList.List.DeepClone();

                        foreach (var item in commitList)
                        {
                            if (item.Point != string.Empty
                                && item.KitNumber != string.Empty
                                && item.PartNumber != string.Empty
                            )
                            {
                                var index = checkInList.List.FindIndex(
                                    n => n.ID == item.ID
                                );

                                if (index != -1)
                                {
                                    var storeContainer = lineData.FirstOrDefault(
                                        n => n.Point == item.Point
                                    );

                                    if (storeContainer != null)
                                    {
                                        HttpClass.HttpUICommand httpUIData = new()
                                        {
                                            ID = string.Empty,
                                            CallID = string.Empty,
                                            Command = new()
                                            {
                                                LoadType = HttpClass.OrderLoadType.SKU,
                                                LocationID = item.Point,
                                                KitID = item.KitNumber,
                                                SKUID = item.PartNumber,
                                                Behaviour = string.Empty,
                                                CommandID = HttpClass.UICommand.Request
                                            }
                                        };

                                        CommonLib.DisplayConsole(
                                           taskName,
                                           $"OnClickOKCommit: httpUIData={CommonLib.JsonSerialize(httpUIData)}",
                                           settingLevel,
                                           MessageLevel.Trace
                                        );

                                        var result = TaskManagerService.AddCommandQueue(httpUIData);
                                    }

                                    checkInList.List.RemoveAt(index);
                                }
                            }
                        }
                    }
                }
            }

            SetPopStatus(false);
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
            if (selectedData != null)
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Are you ready to";
                popMsgData.Text_2 = $"Cancel \"{selectedData.KitNumber} , {selectedData.Point}\" ?";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_1 = () => OnClickOKCancel();
                popMsgData.BtnClick_2 = () => OnClickNok();
                SetPopStatus(true);
            }

            await JsInteropHelper.FocusMyHeader(jsRuntime);
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

    public async void OnClickOKCancel()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickCancel triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (selectedData != null && checkInList?.List?.Count > 0)
            {
                lock (lockCheckInList)
                    checkInList.List.Remove(selectedData);

                ClearKeyboard(selectedData.ID);

                // Add the location back to the available dropdown options
                dropdownOptions.Add(selectedData.Point);
                dropdownOptions = dropdownOptions.OrderBy(loc => loc).ToList();

                selectedData = null!;
            }

            SetPopStatus(false);
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

    public async void OnClickCancelAll()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickCancelAll triggered",
           settingLevel,
           MessageLevel.Information
       );

        try
        {
            if (checkInList?.List?.Count > 0)
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Are you ready to";
                popMsgData.Text_2 = "Cancel All ?";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => OnClickOKCancelAll();
                popMsgData.BtnText_2 = "Cancel";
                popMsgData.BtnClick_2 = () => OnClickNok();
                SetPopStatus(true);
            }

            await JsInteropHelper.FocusMyHeader(jsRuntime);
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

    public async void OnClickOKCancelAll()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickCancelAll triggered",
           settingLevel,
           MessageLevel.Information
       );

        try
        {
            checkInList.List = new();
            selectedData = null!;
            ResetLocations();
            CleaAllKeyboard();

            SetPopStatus(false);
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

    public async void OnClickManual()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"OnClickManual triggered",
           settingLevel,
           MessageLevel.Information
       );

        try
        {
            await JsInteropHelper.FocusMyHeader(jsRuntime);
            modalContent = "CheckInManual";
            showModal = true;
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

    private void RemoveLocation(string location)
    {
        dropdownOptions = dropdownOptions.Where(
            loc => loc != location
        ).ToList();
    }

    private void ResetLocations()
    {
        dropdownOptions = lineData.Select(
            ld => ld.Point
        ).ToList();
        dropdownOptions = dropdownOptions.OrderBy(
            loc => loc
        ).ToList();
    }

    private void ShowModalChanged(bool show)
    {
        showModal = show;
    }

    private async void OnCheckIn(MattelClass.CheckInData checkInData)
    {
        if (checkInData != null)
        {
            if (checkInList.List.Count > 7)
            {
                // Show error message if the already scan 7 positions
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "Already scan more";
                popMsgData.Text_2 = "than 7 locations";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            var findCheckIn = checkInList.List.FirstOrDefault(
                n => n.Point == checkInData.Point
            );

            if (findCheckIn != null)
            {
                // Show error message if the location already scan
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = $"Location \"{findCheckIn.Point}\"";
                popMsgData.Text_2 = "already scan";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Check if the location is empty
            var selectedLineData = lineData.FirstOrDefault(
                ld => ld.Point == checkInData.Point
            );

            if (selectedLineData != null)
            {
                if (selectedLineData.ContainerType == "CheckOut")
                {
                    // Show error message if the location is not empty
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Location \"{selectedLineData.Point}\" already";
                    popMsgData.Text_2 = "use for check out";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
                else if (selectedLineData.ContainerType == "LoadOut")
                {
                    // Show error message if the location is not empty
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Location \"{selectedLineData.Point}\" already";
                    popMsgData.Text_2 = "use for load out";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
                else if (selectedLineData.ContainerType == "Empty")
                {
                    // Show error message if the location is not empty
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Location \"{selectedLineData.Point}\" don't";
                    popMsgData.Text_2 = "have empty table";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
                else if (selectedLineData.ContainerType == "Loaded"
                    || selectedLineData.ContainerType == "Full"
                )
                {
                    // Show error message if the location is not empty
                    popMsgData.ActivatePopup = true;
                    popMsgData.Text_1 = $"Location \"{selectedLineData.Point}\"";
                    popMsgData.Text_2 = "already loaded";
                    popMsgData.Middle_Icon = "icon_issue";
                    popMsgData.BtnText_1 = "OK";
                    popMsgData.BtnClick_1 = async () => await CloseCheckInModal();
                    popMsgData.BtnText_2 = null!;
                    popMsgData.BtnClick_2 = null!;
                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            // If the data is unique, proceed to add the new data
            checkInData.ID =
                checkInList.List.Count != 0
                ? checkInList.List.Max(p => p.ID) + 1
                : 1;
            checkInList.List.Add(checkInData);

            // Remove the used location from the available locations list 
            // RemoveLocation(checkInData.Point);

            showModal = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CloseCheckInModal()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged); // Close the modal
        modalContent = "CheckInManual";
        showModal = true;
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
            SetPopStatus(false);
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

    public async void OnClickManualLocation(int Location)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnClickManualLocation triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var keyboard = keyboards.FirstOrDefault(
                n => n.Guid == pageGuid.ToString()
            );

            if (keyboard != null)
            {
                if (!keyboard.Current.KeyAlias
                    && !keyboard.Current.KeyKit
                    && !keyboard.Current.KeyPart
                    && !keyboard.Current.KeyLocation
                )
                {
                    CommonLib.DisplayConsole(
                       taskName,
                       $"OnClickManualLocation: Location={Location}, pageGuid={pageGuid}",
                       settingLevel,
                       MessageLevel.Information
                    );

                    keyboard.Current.KeyAlias = true;
                    keyboard.Current.KeyLocation = true;
                    keyboard.IsTriggerEnter = true;
                    keyboard.Current.CaptureLocation = Location.ToString();
                    keyboard.Current.DoneLocation = true;
                    ExtendedKeyboardEventArgs e = new()
                    {
                        Guid = pageGuid.ToString()
                    };
                    await JsKeyDown(e);
                }
            }

            await JsInteropHelper.FocusMyHeader(jsRuntime);
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
                    $"ContainerUpdate: Containers={CommonLib.JsonSerialize(Containers)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                lock (lockLineData)
                {
                    foreach (var item in Containers)
                    {
                        var findData = lineData.FirstOrDefault(
                            n => n.Point == item.Location
                        );

                        if (findData != null)
                        {
                            if (item.Status == HttpClass.StatusType.LoadOut)
                            {
                                findData.ContainerType = "LoadOut";
                            }
                            else if (item.LocCondition == HttpClass.ConditionType.CheckOut)
                            {
                                findData.ContainerType = "CheckOut";
                            }
                            else if (item.ContainerId == string.Empty)
                            {
                                findData.ContainerType = "Empty";
                            }
                            else if (item.Status == HttpClass.StatusType.Empty)
                            {
                                findData.ContainerType = "Table";
                            }
                            else if (item.Status == HttpClass.StatusType.Loaded
                                || item.Status == HttpClass.StatusType.Full
                            )
                            {
                                findData.ContainerType = "Loaded";
                            }
                            else if (item.Status == HttpClass.StatusType.LoadOut)
                            {
                                findData.ContainerType = "LoadOut";
                            }
                            else
                            {
                                findData.ContainerType = "NA";
                            }
                        }
                    }

                    SetDynamicComponent(
                        "CheckIn",
                        lineData
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
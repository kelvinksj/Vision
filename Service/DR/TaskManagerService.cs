using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Service.Setting;

namespace TaskManagerWeb.Components.Service.DR;
public class TaskManagerService : IHostedService, IDisposable
{
    private string taskName = "TaskManagerService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_TaskManagerService;
    private bool disposed = false;
    private CommonLib? commonLib;
    private HttpClient? httpClient;
    private SettingsService? settingsService;

    private PeriodicInvoker? periodicInvoker1;
    private ConcurrentQueue<HttpClass.HttpUICommand> httpUIList
        = [];

    private PeriodicInvoker? periodicInvoker2;
    private bool sendHttp2 = false;
    private ConcurrentQueue<HttpClass.HttpUISubscribeCommand> uISubscribeList
        = [];
    private readonly object lockUISubscribers = new();
    private List<(Action<List<HttpClass.UIMessage>>, string)> uISubscribers
        = [];
    private CommonMethods.MonitorParameters uIMonitorParameters = new();
    private readonly object lockTagSubscribers = new();
    private List<(Action<List<string>>, string)> tagSubscribers
        = [];

    private PeriodicInvoker? periodicInvoker3;
    private bool sendHttp3 = false;
    private ConcurrentQueue<HttpClass.HttpUICancelCommand> cancelMessageList
        = [];

    private PeriodicInvoker? periodicInvoker4;
    private bool sendHttp4 = false;
    private ConcurrentQueue<HttpClass.HttpAmrSubscribeCommand> aMRSubscribeList
        = [];
    private readonly object lockAMRSubscribers = new();
    private List<(Action<List<HttpClass.AMR>>, Guid)> aMRSubscribers
        = [];
    private CommonMethods.MonitorParameters aMRMonitorParameters = new();

    private PeriodicInvoker? periodicInvoker5;
    private bool sendHttp5 = false;
    private ConcurrentQueue<HttpClass.HttpAMRCommand> httpAMRList
        = [];

    private PeriodicInvoker? periodicInvoker6;
    private bool sendHttp6 = false;
    private ConcurrentQueue<HttpClass.HttpTableCommand> httpTableList
        = [];

    public TaskManagerService(
        CommonLib CommonLib,
        HttpClient HttpClient,
        SettingsService SettingsService
    )
    {
        commonLib?.DisplayConsole(
            taskName,
            $"TaskManagerService triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            httpClient = HttpClient;
            settingsService = SettingsService;
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
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"StartAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            periodicInvoker1 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker1.StartPeriodicInvoke(Thread1);

            periodicInvoker2 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker2.StartPeriodicInvoke(Thread2);

            periodicInvoker3 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker3.StartPeriodicInvoke(Thread3);

            periodicInvoker4 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker4.StartPeriodicInvoke(Thread4);

            periodicInvoker5 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker5.StartPeriodicInvoke(Thread5);

            periodicInvoker6 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker6.StartPeriodicInvoke(Thread6);

            uIMonitorParameters.OnTimeout = () => AddUISubscriberQueue();
            uIMonitorParameters.TaskName = $"{taskName}: UI:";
            uIMonitorParameters.MessageLevel = settingLevel;
            uIMonitorParameters.AddTimeout = 2000;
            CommonMethods.MonitorSubscribe(uIMonitorParameters);

            aMRMonitorParameters.OnTimeout = () => AddAMRSubscriberQueue();
            aMRMonitorParameters.TaskName = $"{taskName}: AMR:";
            aMRMonitorParameters.MessageLevel = settingLevel;
            aMRMonitorParameters.AddTimeout = 2000;
            CommonMethods.MonitorSubscribe(aMRMonitorParameters);

            await AddUISubscriberQueue();
            await AddAMRSubscriberQueue();
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

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"StopAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            try { await periodicInvoker1!.DisposeAsync(); } catch { }
            try { await periodicInvoker2!.DisposeAsync(); } catch { }
            try { await periodicInvoker3!.DisposeAsync(); } catch { }
            try { await periodicInvoker4!.DisposeAsync(); } catch { }
            try { await periodicInvoker5!.DisposeAsync(); } catch { }
            try { await periodicInvoker6!.DisposeAsync(); } catch { }
            uIMonitorParameters.CancellationTokenSource.Cancel();
            aMRMonitorParameters.CancellationTokenSource.Cancel();
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

        await Task.CompletedTask;
    }

    public void Dispose()
    // Need to add "@implements IDisposable" at the top of the razor file
    // and add ": IDisposable" after partial class on the same line
    {
        commonLib?.DisplayConsole(
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
            commonLib?.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    protected async virtual void DisposeAsync(bool Disposing)
    {
        commonLib?.DisplayConsole(
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
                    commonLib?.DisplayConsole(
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
    ~TaskManagerService()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    public void SubscribeMessage(Action<List<HttpClass.UIMessage>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeMessage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockUISubscribers)
                if (!uISubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == AreaID
                ))
                    uISubscribers.Add((Handler, AreaID));
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
    }

    public void UnsubscribeMessage(Action<List<HttpClass.UIMessage>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UnsubscribeMessage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockUISubscribers)
            {
                var subscriber = uISubscribers.FirstOrDefault(
                    s => s.Item1 == Handler && s.Item2 == AreaID
                );

                if (subscriber != default)
                {
                    uISubscribers.Remove(subscriber);
                }
            }
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
    }

    public async Task ClientUpdateMessage(List<HttpClass.UIMessage> UIMessageList)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ClientUpdateMessage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (UIMessageList?.Count > 0)
            {
                foreach (var item in UIMessageList)
                {
                    if (string.IsNullOrWhiteSpace(item.AreaID))
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ClientUpdateMessage: [UIMessage Error] area ID is empty:"
                            + $" TaskID={item.TaskID}, AreaID={item.AreaID}",
                            settingLevel,
                            MessageLevel.Error
                        );
                        item.AreaID = "Error";
                    }
                    else if (item.AreaID.Length != 4)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ClientUpdateMessage: [UIMessage Error] area ID lenght must be exactly 4"
                            + $" characters: TaskID={item.TaskID}, AreaID={item.AreaID}",
                            settingLevel,
                            MessageLevel.Error
                        );
                    }
                }
            }
            else
            {
                UIMessageList = [];
            }

            uIMonitorParameters.IsAlive = true;
            var groupedMessages = UIMessageList
                .GroupBy(n => n.AreaID)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            lock (lockUISubscribers)
            {
                foreach (var (handler, areaID) in uISubscribers)
                {
                    if (groupedMessages.TryGetValue(areaID, out var filteredList)
                        && filteredList?.Count > 0
                    )
                        handler(filteredList);
                    else
                        handler([]);
                }
            }
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

        await Task.CompletedTask;
    }

    public void SubscribeTag(Action<List<string>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeTag triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockTagSubscribers)
                if (!tagSubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == AreaID
                ))
                    tagSubscribers.Add((Handler, AreaID));
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
    }

    public void UnsubscribeTag(Action<List<string>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UIUnsubscribeTag triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockTagSubscribers)
            {
                var subscriber = tagSubscribers.FirstOrDefault(
                    s => s.Item1 == Handler && s.Item2 == AreaID
                );

                if (subscriber != default)
                {
                    tagSubscribers.Remove(subscriber);
                }
            }
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
    }

    public async Task ClientUpdateTag(List<HttpClass.UIMessage> UIMessageList)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ClientUpdateTag triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (UIMessageList?.Count > 0)
            {
                foreach (var item in UIMessageList)
                {
                    if (string.IsNullOrWhiteSpace(item.LocationID))
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ClientUpdateTag: [UIMessage Error] location ID is empty:"
                            + $" TaskID={item.TaskID}, LocationID={item.LocationID}",
                            settingLevel,
                            MessageLevel.Disable
                        );
                        item.LocationID = "Error";
                    }
                    else if (item.LocationID.Length != 8)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ClientUpdateTag: [UIMessage Error] location ID lenght must be exactly 8"
                            + $" characters: TaskID={item.TaskID}, LocationID={item.LocationID}",
                            settingLevel,
                            MessageLevel.Error
                        );
                    }
                }
            }
            else
            {
                UIMessageList = [];
            }

            var groupedMessages = UIMessageList
                .GroupBy(n => n.LocationID[..4])
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(n => n.LocationID).ToList()
                );

            foreach (var data in groupedMessages)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"ContainerClientUpdate: Key={data.Key.ToString()}",
                    settingLevel,
                    MessageLevel.Disable
                );
            }

            lock (lockUISubscribers)
            {
                foreach (var (handler, areaID) in tagSubscribers)
                {
                    if (groupedMessages.TryGetValue(areaID, out var filteredList)
                        && filteredList?.Count > 0
                    )
                        handler(filteredList);
                    else
                        handler([]);
                }
            }
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

        await Task.CompletedTask;
    }

    private async Task InvokeAsyncWrapper(
        Func<CancellationToken, Task> Action,
        CancellationToken CancellationToken
    )
    {
        try
        {
            if (Action == null) throw new ArgumentException(nameof(Action));

            await Action(CancellationToken);
        }
        catch (Exception ex)
        {
            commonLib?.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            throw;
        }
    }

    private async Task Thread1(CancellationToken CancellationToken)
    {
        try
        {
            string threadName = "Thread1";
            bool sendHttp = false;
            bool canRemove = false;

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp && httpUIList.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{threadName}: Count={httpUIList.Count}, "
                        + $"httpUIList={commonLib?.JsonSerialize(httpUIList)}",
                        settingLevel,
                        MessageLevel.Disable
                    );
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{threadName}: Count={httpUIList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Disable
                    );

                    sendHttp = true;
                    (sendHttp, canRemove) = await SendHttp1(outData, CancellationToken);

                    if (canRemove)
                    {
                        canRemove = false;
                        httpUIList.TryDequeue(out var tmpData);
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{threadName}: Count={httpUIList.Count}, "
                            + $"tmpData={commonLib?.JsonSerialize(tmpData!)}",
                            settingLevel,
                            MessageLevel.Disable
                        );
                    }
                }
            }
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

        await Task.CompletedTask;
    }

    private async Task<(bool, bool)> SendHttp1(
        HttpClass.HttpUICommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp1";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uicommand}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uicommand,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpUIResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpUIResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return (isLoop, canRemove);
    }

    private async Task Thread2(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp2 && uISubscribeList.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread2: Count={uISubscribeList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp2 = true;
                    sendHttp2 = await SendHttp2(outData, CancellationToken);
                }
            }
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
    }

    private async Task<bool> SendHttp2(
        HttpClass.HttpUISubscribeCommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp2";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                HttpCommand.UrlData.IsHttps
                    = HttpParameters.MyHeader.Contains(
                        "https",
                        StringComparison.OrdinalIgnoreCase
                    ) ? true : false;
                HttpCommand.UrlData.Ip = HttpParameters.MyIP;
                HttpCommand.UrlData.Port = HttpParameters.MyPort;
                HttpCommand.UrlData.Endpoint = "Api/TaskManager/Messages";

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uisubscribe}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uisubscribe,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpUISubscribeResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpUISubscribeResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            uIMonitorParameters.IntervalTime = HttpCommand.RequestBehaviour.IntervalTime;
                            uIMonitorParameters.IsStart = true;

                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (canRemove)
            {
                canRemove = false;
                uISubscribeList.TryDequeue(out var outData);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} remove queue",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return isLoop;
    }

    private async Task Thread3(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp3
                    && cancelMessageList.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread3: Count={cancelMessageList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp3 = true;
                    sendHttp3 = await SendHttp3(outData, CancellationToken);
                }
            }
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
    }

    private async Task<bool> SendHttp3(
        HttpClass.HttpUICancelCommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp3";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uicancel}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uicancel,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpUICancelResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpUICancelResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (canRemove)
            {
                canRemove = false;
                cancelMessageList.TryDequeue(out var outData);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} remove queue",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return isLoop;
    }

    private async Task Thread4(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp4
                    && aMRSubscribeList.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread4: Count={aMRSubscribeList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp4 = true;
                    sendHttp4 = await SendHttp4(outData, CancellationToken);
                }
            }
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
    }

    private async Task<bool> SendHttp4(
        HttpClass.HttpAmrSubscribeCommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp4";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                HttpCommand.UrlData.IsHttps
                    = HttpParameters.MyHeader.Contains(
                        "https",
                        StringComparison.OrdinalIgnoreCase
                    ) ? true : false;
                HttpCommand.UrlData.Ip = HttpParameters.MyIP;
                HttpCommand.UrlData.Port = HttpParameters.MyPort;
                HttpCommand.UrlData.Endpoint = "Api/TaskManager/AMR";

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uiamrsubscribe}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uiamrsubscribe,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpAmrSubscribeResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpAmrSubscribeResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            aMRMonitorParameters.IntervalTime = HttpCommand.RequestBehaviour.IntervalTime;
                            aMRMonitorParameters.IsStart = true;

                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (canRemove)
            {
                canRemove = false;
                aMRSubscribeList.TryDequeue(out var outData);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} remove queue",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return isLoop;
    }

    private async Task Thread5(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp5 && httpAMRList.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread5: Count={httpAMRList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp5 = true;
                    sendHttp5 = await SendHttp5(outData, CancellationToken);
                }
            }
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
    }

    private async Task<bool> SendHttp5(
        HttpClass.HttpAMRCommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp5";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uiamrcommand}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uiamrcommand,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpAMRResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpAMRResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (canRemove)
            {
                canRemove = false;
                httpAMRList.TryDequeue(out var outData);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} remove queue",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return isLoop;
    }

    private async Task Thread6(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp6 && httpTableList.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread6: Count={httpTableList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp6 = true;
                    sendHttp6 = await SendHttp6(outData, CancellationToken);
                }
            }
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
    }

    private async Task<bool> SendHttp6(
        HttpClass.HttpTableCommand HttpCommand,
        CancellationToken CancellationToken
    )
    {
        string httpName = "SendHttp6";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        int sendID = HttpParameters.CallID;
        bool isLoop = true;
        bool canRemove = false;
        string callID = $"{commonLib?.GetDateTime()}-{sendID}";

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ServiceEndpoint_uitablecommand}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(HttpCommand)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        HttpParameters.ServiceEndpoint_uitablecommand,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.HttpTableResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.HttpTableResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data OK",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} data error",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }

                        canRemove = true;
                        isLoop = false;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Information
                        );

                        canRemove = true;
                        isLoop = false;
                    }
                }
                else
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: result={result?.StatusCode}",
                        settingLevel,
                        MessageLevel.Information
                    );

                    canRemove = true;
                    isLoop = false;
                }
            }
            catch (HttpRequestException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (TaskCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request timed out: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} invalid operation: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (OperationCanceledException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} request was canceled: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (SerializationException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} serialization error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (JsonException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} json error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
                );
            }
            catch (NotSupportedException ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} not supported error: [{ex.HResult}]{ex.Message}",
                    settingLevel,
                    MessageLevel.Error
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

                throw new ArgumentException(ex.Message);
            }

            if (canRemove)
            {
                canRemove = false;
                httpTableList.TryDequeue(out var outData);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName} remove queue",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            if (isLoop)
            {
                await Task.Delay(intervalTime, CancellationToken);
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: intervalTime={intervalTime}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (intervalTime < finalTime)
                    intervalTime += (int)(intervalTime * 0.2);
            }
        }

        return isLoop;
    }

    public async Task<bool> AddTableCommandQueue(HttpClass.HttpTableCommand HttpTableCommand)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddTableCommandQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"AddTableCommandQueue: HttpTableCommand={commonLib?.JsonSerialize(HttpTableCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            httpTableList.Enqueue(HttpTableCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddTableCommandQueue: Count={httpTableList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> AddAMRCommandQueue(HttpClass.HttpAMRCommand HttpAMRCommand)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddAMRCommandQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"AddAMRCommandQueue: HttpAMRCommand={commonLib?.JsonSerialize(HttpAMRCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            httpAMRList.Enqueue(HttpAMRCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddAMRCommandQueue: Count={httpAMRList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> AddCommandQueue(HttpClass.HttpUICommand HttpCommand)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddCommandQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"AddCommandQueue: HttpCommand={commonLib?.JsonSerialize(HttpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            httpUIList.Enqueue(HttpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddCommandQueue: httpUIList={commonLib?.JsonSerialize(httpUIList)}",
                settingLevel,
                MessageLevel.Disable
            );
            commonLib?.DisplayConsole(
                taskName,
                $"AddCommandQueue: Count={httpUIList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> AddUISubscriberQueue()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddUISubscriberQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            uIMonitorParameters.IsStart = false;
            HttpClass.HttpUISubscribeCommand httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                RequestBehaviour = new()
                {
                    Behaviour = HttpClass.BehaviourType.Pooling,
                    IntervalTime = 1000,
                    SubscriptionTime = 1,
                    Filter = string.Empty
                },
                AreaId = [
                    "0000"
                ]
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddUISubscriberQueue: HttpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            uISubscribeList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddUISubscriberQueue: Count={uISubscribeList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> AddCancelMessageQueue(string TaskID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddCancelMessageQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"AddCancelMessageQueue: TaskID={TaskID}",
                settingLevel,
                MessageLevel.Trace
            );
            HttpClass.HttpUICancelCommand HttpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                TaskID = TaskID,
                CommandID = "Cancel"
            };
            cancelMessageList.Enqueue(HttpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddCancelMessageQueue: Count={cancelMessageList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> AddAMRSubscriberQueue()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddAMRSubscriberQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            aMRMonitorParameters.IsStart = false;
            HttpClass.HttpAmrSubscribeCommand httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                RequestBehaviour = new()
                {
                    Behaviour = HttpClass.BehaviourType.Pooling,
                    IntervalTime = 1000,
                    SubscriptionTime = 1,
                    Filter = string.Empty
                },
                AMR = [
                    "0000"
                ]
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddAMRSubscriberQueue: HttpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            aMRSubscribeList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddAMRSubscriberQueue: Count={aMRSubscribeList.Count}",
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
            throw;
        }

        return await Task.FromResult(true);
    }

    public void AMRSubscribe(Action<List<HttpClass.AMR>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AMRSubscribe triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockAMRSubscribers)
                if (!aMRSubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == Guid
                ))
                    aMRSubscribers.Add((Handler, Guid));
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
    }

    public void AMRUnsubscribe(Action<List<HttpClass.AMR>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AMRUnsubscribe triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockAMRSubscribers)
            {
                var subscriber
                    = aMRSubscribers.FirstOrDefault(s =>
                        s.Item1 == Handler && s.Item2 == Guid
                    );

                if (subscriber != default)
                {
                    aMRSubscribers.Remove(subscriber);
                }
            }
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
    }

    public async Task AMRClientUpdate(List<HttpClass.AMR> AMR)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AMRClientUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            aMRMonitorParameters.IsAlive = true;

            if (AMR?.Count > 0)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"AMRClientUpdate got data",
                    settingLevel,
                    MessageLevel.Information
                );

                lock (lockAMRSubscribers)
                {
                    foreach (var (handler, guid) in aMRSubscribers)
                    {
                        handler(AMR);
                    }
                }
            }
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

        await Task.CompletedTask;
    }
}
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Setting;

namespace TaskManagerWeb.Components.Service.DR;
public class ContainerService : IHostedService, IDisposable
{
    private string taskName = "ContainerService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ContainerService;
    private bool disposed = false;
    private CommonLib? commonLib;
    private HttpClient? httpClient;
    private SettingsService? settingsService;
    private MattelService? mattelService;
    private CommonMethods.MonitorParameters monitorParameters = new();

    private PeriodicInvoker? periodicInvoker1;
    private bool sendHttp1 = false;
    private ConcurrentQueue<HttpClass.LocationStatusUpdateSubscriber> loccationSubscriberList
        = [];

    private PeriodicInvoker? periodicInvoker2;
    private bool sendHttp2 = false;
    private ConcurrentQueue<HttpClass.CancelSubscription> cancelSubscriptionList
        = [];

    private PeriodicInvoker? periodicInvoker3;
    private bool sendHttp3 = false;
    private ConcurrentQueue<HttpClass.SettingCommand> settingCommand
        = [];

    private PeriodicInvoker? periodicInvoker4;
    private ConcurrentQueue<HttpClass.ContainerCommand> containerCommand
        = [];

    private readonly object lockSubscribeIdList = new();
    private MattelClass.SubscribeIdList subscribeIdList = new();

    private List<string> checkInLocation = [];

    private readonly object lockContainerSubscribers = new();
    private List<(Action<List<HttpClass.ContainerData>>, string)> containerSubscribers
        = [];

    private readonly object lockQuerySubscribers = new();
    private List<(Action<List<HttpClass.ContainerData>>, Guid)> querySubscribers
        = [];

    public ContainerService(
        CommonLib CommonLib,
        HttpClient HttpClient,
        SettingsService SettingsService,
        MattelService MattelService
    )
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ContainerService triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            httpClient = HttpClient;
            settingsService = SettingsService;
            mattelService = MattelService;
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

            monitorParameters.OnTimeout = () => AddLocationSubscriberQueue(checkInLocation);
            monitorParameters.TaskName = taskName;
            monitorParameters.MessageLevel = settingLevel;
            monitorParameters.AddTimeout = 2000;
            CommonMethods.MonitorSubscribe(monitorParameters);

            checkInLocation = new()
            {
                "2001",
                "2101",
                "2201",
                "2202",
                "2203",
                "2204",
                "2205",
                "2206",
                "2300",
                "2301",
                "2302",
                "2303",
                "2304",
                "2305",
                "2306",
                "2401"
            };
            _ = AddLocationSubscriberQueue(checkInLocation);
            _ = AddSettingCommand();
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

            monitorParameters.CancellationTokenSource.Cancel();
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
    ~ContainerService()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    public void SubscribeContainer(Action<List<HttpClass.ContainerData>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeContainer triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockContainerSubscribers)
                if (!containerSubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == AreaID
                ))
                    containerSubscribers.Add((Handler, AreaID));
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

    public void UnsubscribeContainer(Action<List<HttpClass.ContainerData>> Handler, string AreaID)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UnsubscribeContainer triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockContainerSubscribers)
            {
                var subscriber
                    = containerSubscribers.FirstOrDefault(s =>
                        s.Item1 == Handler && s.Item2 == AreaID
                    );

                if (subscriber != default)
                {
                    containerSubscribers.Remove(subscriber);
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

    public async Task ContainerClientUpdate(List<HttpClass.ContainerData> ContainerDatas)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ContainerClientUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            monitorParameters.IsAlive = true;

            if (ContainerDatas?.Count > 0)
            {
                foreach (var item in ContainerDatas)
                {
                    if (string.IsNullOrWhiteSpace(item.Location))
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ContainerClientUpdate: [Container Error] location is empty:"
                            + $" ContainerId={item.ContainerId}, Location={item.Location},"
                            + $" IndexDateTime={item.IndexDateTime}",
                            settingLevel,
                            MessageLevel.Error
                        );
                        item.Location = "Error";
                    }
                    else if (item.Location.Length != 8)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            "ContainerClientUpdate: [Container Error] location lenght must be exactly 8"
                            + $" ContainerId={item.ContainerId}, Location={item.Location},"
                            + $" IndexDateTime={item.IndexDateTime}",
                            settingLevel,
                            MessageLevel.Error
                        );
                    }
                }

                var groupedContainers = ContainerDatas
                    .GroupBy(n => n.Location[..4])
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList()
                    );

                foreach (var data in groupedContainers)
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"ContainerClientUpdate: Key={data.Key.ToString()}",
                        settingLevel,
                        MessageLevel.Disable
                    );
                    foreach (var d in data.Value)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"ContainerClientUpdate: Key={d.Location}",
                            settingLevel,
                            MessageLevel.Disable
                        );
                    }
                }

                commonLib?.DisplayConsole(
                    taskName,
                    $"ContainerClientUpdate:  groupedContainers={commonLib?.JsonSerialize(groupedContainers)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                lock (lockContainerSubscribers)
                {
                    foreach (var (handler, areaID) in containerSubscribers)
                    {
                        if (groupedContainers.TryGetValue(areaID, out var filteredList)
                            && filteredList?.Count > 0
                        )
                            handler(filteredList);
                        else
                            handler([]);
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

    public void SubscribeQuery(Action<List<HttpClass.ContainerData>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeQuery triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockQuerySubscribers)
                if (!querySubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == Guid
                ))
                    querySubscribers.Add((Handler, Guid));
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

    public void UnsubscribeQuery(Action<List<HttpClass.ContainerData>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UnsubscribeQuery triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockQuerySubscribers)
            {
                var subscriber
                    = querySubscribers.FirstOrDefault(s =>
                        s.Item1 == Handler && s.Item2 == Guid
                    );

                if (subscriber != default)
                {
                    querySubscribers.Remove(subscriber);
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

    public async Task QueryClientUpdate(List<HttpClass.ContainerData> ContainerDatas, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"QueryClientUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            ContainerDatas ??= [];

            commonLib?.DisplayConsole(
                taskName,
                $"QueryClientUpdate got data",
                settingLevel,
                MessageLevel.Information
            );

            lock (lockQuerySubscribers)
            {
                foreach (var (handler, guid) in querySubscribers)
                {
                    if (guid == Guid)
                        handler(ContainerDatas);
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

    private async Task InvokeAsyncWrapper(Func<CancellationToken, Task> Action, CancellationToken CancellationToken)
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
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp1
                    && loccationSubscriberList.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread1: Count={loccationSubscriberList.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    sendHttp1 = true;
                    sendHttp1 = await SendHttp1(outData, CancellationToken);
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

    private async Task<bool> SendHttp1(
        HttpClass.LocationStatusUpdateSubscriber HttpCommand,
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
        bool isQuery =
        (
            HttpCommand.RequestBehaviour.Behaviour == HttpClass.BehaviourType.CheckOut
            || HttpCommand.RequestBehaviour.Behaviour == HttpClass.BehaviourType.Maintenance
        );

        while (isLoop && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                HttpCommand.ID = "TaskManagerWeb";
                HttpCommand.CallID = callID;

                if (isQuery)
                {
                    HttpCommand.UrlData = new();
                }
                else
                {
                    HttpCommand.UrlData.IsHttps
                        = HttpParameters.MyHeader.Contains(
                            "https",
                            StringComparison.OrdinalIgnoreCase
                        ) ? true : false;
                    HttpCommand.UrlData.Ip = HttpParameters.MyIP;
                    HttpCommand.UrlData.Port = HttpParameters.MyPort;
                    HttpCommand.UrlData.Endpoint = "Api/Container/LocationUpdate";
                }

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ContainerEndpoint_locationSubPub}",
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
                        HttpParameters.ContainerEndpoint_locationSubPub,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.StatusUpdateResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.StatusUpdateResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: Code={data.Code}, ResponseId={data.ResponseId}, SubscribeId={data.SubscribeId}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            if (isQuery)
                            {
                                await QueryClientUpdate(data.Data, HttpCommand.Guid);
                            }
                            else
                            {
                                monitorParameters.IntervalTime = HttpCommand.RequestBehaviour.IntervalTime;
                                monitorParameters.IsStart = true;

                                lock (lockSubscribeIdList)
                                {
                                    var findData = subscribeIdList.SubscribeId.FirstOrDefault(
                                        n => n == data.SubscribeId
                                    );

                                    if (findData == null)
                                    {
                                        // subscribeIdList.SubscribeId.Add(data.SubscribeId);
                                    }
                                }

                                await ContainerClientUpdate(data.Data);
                            }

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
                loccationSubscriberList.TryDequeue(out var outData);
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

    private async Task Thread2(CancellationToken CancellationToken)
    {
        try
        {
            await Task.Delay(0, CancellationToken);

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp2
                    && cancelSubscriptionList.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread2: Count={cancelSubscriptionList.Count}, "
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
        HttpClass.CancelSubscription HttpCommand,
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

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ContainerEndpoint_cancelSubscription}",
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
                        HttpParameters.ContainerEndpoint_cancelSubscription,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.GenericReply? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.GenericReply>(
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
                cancelSubscriptionList.TryDequeue(out var outData);
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
                    && settingCommand.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread3: Count={settingCommand.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Disable
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
        HttpClass.SettingCommand HttpCommand,
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
                    $"{httpName}: endpoint={HttpParameters.ContainerEndpoint_uicommand}",
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
                        HttpParameters.ContainerEndpoint_uicommand,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.GenericReply? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.GenericReply>(
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
                settingCommand.TryDequeue(out var outData);
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
            string threadName = "Thread4";
            bool sendHttp = false;
            bool canRemove = false;

            if (!CancellationToken.IsCancellationRequested)
            {
                if (!sendHttp && containerCommand.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{threadName}: Count={containerCommand.Count}, "
                        + $"httpUIList={commonLib?.JsonSerialize(containerCommand)}",
                        settingLevel,
                        MessageLevel.Disable
                    );
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{threadName}: Count={containerCommand.Count}, "
                        + $"outData={commonLib?.JsonSerialize(outData)}",
                        settingLevel,
                        MessageLevel.Disable
                    );

                    sendHttp = true;
                    (sendHttp, canRemove) = await SendHttp4(outData, CancellationToken);

                    if (canRemove)
                    {
                        canRemove = false;
                        containerCommand.TryDequeue(out var tmpData);
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{threadName}: Count={containerCommand.Count}, "
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
    }

    private async Task<(bool, bool)> SendHttp4(
        HttpClass.ContainerCommand HttpCommand,
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

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ContainerEndpoint_uicommand}",
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
                        HttpParameters.ContainerEndpoint_uicommand,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.GenericReply? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.GenericReply>(
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

    public async Task<bool> AddContainerCommand(Element.Table Table)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddSettingCommand triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            HttpClass.ContainerCommand httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                CommandID = HttpClass.UICommand.ContainerUpdate,
                ContainerUpdateData = new()
                {
                    ContainerId = Table.TableCode,
                    Location = Table.Location,
                    Status = Table.Status,
                    LoadType = Table.Type,
                    Condition = HttpClass.ConditionType.Normal,
                    LocCondition = HttpClass.ConditionType.Normal,
                    KitId = Table.KitNumber,
                    SkuId = Table.PartNumber
                }
            };

            if (httpCommand.ContainerUpdateData.Status == HttpClass.ConditionType.Quarantine)
            {
                httpCommand.ContainerUpdateData.Status = HttpClass.StatusType.Loaded;
                httpCommand.ContainerUpdateData.Condition = HttpClass.ConditionType.Quarantine;
            }

            if (Table.LocCondition == Element.TableState.LoadOut)
            {
                httpCommand.ContainerUpdateData.LoadType = HttpClass.OrderLoadType.NA;
                httpCommand.ContainerUpdateData.Status = HttpClass.StatusType.Empty;
                httpCommand.ContainerUpdateData.Condition = HttpClass.ConditionType.Normal;
            }

            commonLib?.DisplayConsole(
                taskName,
                $"AddSettingCommand: httpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            containerCommand.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddSettingCommand: Count={containerCommand.Count}",
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

    public async Task<bool> AddSettingCommand()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddSettingCommand triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            var cPCSettings = mattelService?.GetCPCSettings() ?? new();
            HttpClass.SettingCommand httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                CommandID = HttpClass.UICommand.LocationSetting
            };

            foreach (var item in cPCSettings.LocationSettings)
            {
                httpCommand.LocationParameter.Add(new()
                {
                    LocationID = item.LocationID,
                    Parameter = item.Parameter
                });
            }

            commonLib?.DisplayConsole(
                taskName,
                $"AddSettingCommand: httpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            settingCommand.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddSettingCommand: Count={settingCommand.Count}",
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

    public void TriggerLocationSubscriberQueue()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"TriggerLocationSubscriber triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            _ = AddLocationSubscriberQueue(checkInLocation);
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

    private async Task<bool> AddLocationSubscriberQueue(List<string> LocationList)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddLocationSubscriberQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            monitorParameters.IsStart = false;
            HttpClass.LocationStatusUpdateSubscriber httpCommand = new()
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
                Location = LocationList,
                Guid = Guid.Empty
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddLocationSubscriberQueue: httpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            loccationSubscriberList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddLocationSubscriberQueue: Count={loccationSubscriberList.Count}",
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

    public async Task<bool> AddCancelSubscriberQueue(List<string> SubscribeId)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"CancelSubscriberQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib?.DisplayConsole(
                taskName,
                $"CancelSubscriberQueue: SubscribeId={commonLib?.JsonSerialize(SubscribeId)}",
                settingLevel,
                MessageLevel.Trace
            );
            HttpClass.CancelSubscription HttpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                SubscribeId = SubscribeId
            };
            cancelSubscriptionList.Enqueue(HttpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"CancelSubscriberQueue: Count={cancelSubscriptionList.Count}",
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

    public async Task<bool> AddFilterQueue(string Behaviour, string Filter, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddFilterQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            HttpClass.LocationStatusUpdateSubscriber httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                RequestBehaviour = new()
                {
                    Behaviour = Behaviour,
                    IntervalTime = 0,
                    SubscriptionTime = 0,
                    Filter = Filter,
                },
                Location = [],
                Guid = Guid
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddFilterQueue: httpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            loccationSubscriberList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddFilterQueue: Count={loccationSubscriberList.Count}",
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
}
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Setting;

namespace TaskManagerWeb.Components.Service.DR;
public class ScanService : IHostedService, IDisposable
{
    private string taskName = "ScanService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ScanService;
    private bool disposed = false;
    private CommonLib? commonLib;
    private HttpClient? httpClient;
    private SettingsService? settingsService;
    private CommonMethods.MonitorParameters monitorParameters = new();
    private bool isScanning = false;
    private bool isConfirmReply = false;
    private bool isScanError = false;

    private PeriodicInvoker? periodicInvoker1;
    private bool sendHttp1 = false;
    private ConcurrentQueue<HttpClass.ScanRequest> scanRequestList
        = [];

    private PeriodicInvoker? periodicInvoker2;
    private bool sendHttp2 = false;
    private ConcurrentQueue<HttpClass.ScanUISubscribeCommand> scanSubscribeList
        = [];

    private readonly object lockScanSubscribers = new();
    private List<(Action<List<MattelClass.Sheet>>, Guid)> scanSubscribers
        = [];

    private readonly object lockUISubscribers = new();
    private List<(Action<List<HttpClass.UIMessage>>, Guid)> uISubscribers
        = [];

    private readonly object lockButtonActive = new();
    private List<(Action<bool>, Guid)> buttonActiveList
        = [];

    public bool IsScanning
    {
        get
        {
            return isScanning;
        }

        set
        {
            isScanning = value;
            commonLib?.DisplayConsole(
                taskName,
                $"IsScanning: isScanning={isScanning}",
                settingLevel,
                MessageLevel.Trace
            );
            ActiveClientUpdate(isScanning);

            if (isScanning)
            {
                ButtonReactiveTimeout(5000);
            }
        }
    }

    public ScanService(
        CommonLib CommonLib,
        HttpClient HttpClient,
        SettingsService SettingsService
    )
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ScanService triggered",
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

            monitorParameters.OnTimeout = () => AddSubscriberMessage();
            monitorParameters.TaskName = taskName;
            monitorParameters.MessageLevel = settingLevel;
            monitorParameters.AddTimeout = 2000;
            CommonMethods.MonitorSubscribe(monitorParameters);

            await AddSubscriberMessage();
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
    ~ScanService()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    public void SubscribeActive(Action<bool> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeActive triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockButtonActive)
                if (!buttonActiveList.Any(
                        s => s.Item1 == Handler && s.Item2 == Guid
                    )
                )
                {
                    buttonActiveList.Add((Handler, Guid));
                    ActiveClientUpdate(isScanning);
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

    public void UnsubscribeActive(Action<bool> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UnsubscribeActive triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockButtonActive)
            {
                var subscriber
                    = buttonActiveList.FirstOrDefault(s =>
                        s.Item1 == Handler && s.Item2 == Guid
                    );

                if (subscriber != default)
                {
                    buttonActiveList.Remove(subscriber);
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

    public void ActiveClientUpdate(bool IsActive)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ActiveClientUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockButtonActive)
            {
                foreach (var (handler, guid) in buttonActiveList)
                {
                    handler(IsActive);
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

    public void SubscribeScan(Action<List<MattelClass.Sheet>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"SubscribeScan triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockScanSubscribers)
                if (!scanSubscribers.Any(s =>
                    s.Item1 == Handler && s.Item2 == Guid
                ))
                    scanSubscribers.Add((Handler, Guid));
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

    public void UnsubscribeScan(Action<List<MattelClass.Sheet>> Handler, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UnsubscribeScan triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            lock (lockScanSubscribers)
            {
                var subscriber
                    = scanSubscribers.FirstOrDefault(s =>
                        s.Item1 == Handler && s.Item2 == Guid
                    );

                if (subscriber != default)
                {
                    scanSubscribers.Remove(subscriber);
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

    public void ScanClientUpdate(List<MattelClass.Sheet> Datas, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ScanClientUpdate triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            if (Datas?.Count > 0)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"ScanClientUpdate got data",
                    settingLevel,
                    MessageLevel.Information
                );

                IsScanning = false;

                lock (lockScanSubscribers)
                {
                    foreach (var (handler, guid) in scanSubscribers)
                    {
                        if (guid == Guid)
                            handler(Datas);
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

    public void SubscribeUI(Action<List<HttpClass.UIMessage>> Handler, Guid Guid)
    {
        lock (lockUISubscribers)
            if (!uISubscribers.Any(s =>
                s.Item1 == Handler && s.Item2 == Guid
            ))
                uISubscribers.Add((Handler, Guid));
    }

    public void UnsubscribeUI(Action<List<HttpClass.UIMessage>> Handler, Guid Guid)
    {
        lock (lockUISubscribers)
        {
            var subscriber
                = uISubscribers.FirstOrDefault(s =>
                    s.Item1 == Handler && s.Item2 == Guid
                );

            if (subscriber != default)
            {
                uISubscribers.Remove(subscriber);
            }
        }
    }

    public async Task MessageUpdate(List<HttpClass.UIMessage> UIMessageList)
    {
        monitorParameters.IsAlive = true;
        var groupedMessages = UIMessageList
            .GroupBy(n => n.Guid)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );

        lock (lockUISubscribers)
        {
            if (!isScanError
                && UIMessageList.Count > 0
                && UIMessageList[0].Title.Text.Contains("Error", StringComparison.OrdinalIgnoreCase)
            )
            {
                isScanError = true;
            }

            if (isScanError
                && ((UIMessageList.Count > 0
                        && !UIMessageList[0].Title.Text.Contains("Error", StringComparison.OrdinalIgnoreCase)
                    )
                    || UIMessageList.Count == 0
                )
            )
            {
                isScanError = false;
                IsScanning = false;
            }

            foreach (var (handler, guid) in uISubscribers)
            {
                if (groupedMessages.TryGetValue(guid, out var filteredList)
                    && filteredList?.Count > 0
                )
                    handler(filteredList);
                else
                    handler([]);
            }
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
                    && scanRequestList.TryPeek(out var outData)
                )
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread1: Count={scanRequestList.Count}, "
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
        HttpClass.ScanRequest HttpCommand,
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
                HttpCommand.UrlData.IsHttps
                    = HttpParameters.MyHeader.Contains(
                        "https",
                        StringComparison.OrdinalIgnoreCase
                    ) ? true : false;
                HttpCommand.UrlData.Ip = HttpParameters.MyIP;
                HttpCommand.UrlData.Port = HttpParameters.MyPort;
                HttpCommand.UrlData.Endpoint = "Api/Scan/ScanUpdate";

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ScanEndpoint_scan}",
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
                        HttpParameters.ScanEndpoint_scan,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.ScanReply? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.ScanReply>(
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
                            isConfirmReply = true;
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
                scanRequestList.TryDequeue(out var outData);
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
                if (!sendHttp2 && scanSubscribeList.TryPeek(out var outData))
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"Thread2: Count={scanSubscribeList.Count}, "
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
        HttpClass.ScanUISubscribeCommand HttpCommand,
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
                HttpCommand.UrlData.Endpoint = "Api/Scan/Messages";

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: endpoint={HttpParameters.ScanEndpoint_scanprogress}",
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
                        HttpParameters.ScanEndpoint_scanprogress,
                        HttpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    HttpClass.ScanUISubscribeResponse? data
                        = await result.Content.ReadFromJsonAsync<HttpClass.ScanUISubscribeResponse>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib?.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Disable
                        );

                        if (data.Code == 1000 && data.ResponseId == HttpCommand.CallID)
                        {
                            monitorParameters.IntervalTime = HttpCommand.RequestBehaviour.IntervalTime;
                            monitorParameters.IsStart = true;

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
                scanSubscribeList.TryDequeue(out var outData);
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

    public async Task<bool> RequestScan(string Type, string Format, Guid Guid)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddLoccationSubscriberQueue triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            HttpClass.ScanRequest httpCommand = new()
            {
                ID = string.Empty,
                CallID = string.Empty,
                Type = Type,
                Format = Format,
                Guid = Guid
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddLoccationSubscriberQueue: httpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            IsScanning = true;
            scanRequestList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddLoccationSubscriberQueue: Count={scanRequestList.Count}",
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

    public async Task<bool> AddSubscriberMessage()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"AddSubscriberMessage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            monitorParameters.IsStart = false;
            HttpClass.ScanUISubscribeCommand httpCommand = new()
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
                    "scan"
                ]
            };
            commonLib?.DisplayConsole(
                taskName,
                $"AddSubscriberMessage: HttpCommand={commonLib?.JsonSerialize(httpCommand)}",
                settingLevel,
                MessageLevel.Trace
            );
            scanSubscribeList.Enqueue(httpCommand.DeepClone());
            commonLib?.DisplayConsole(
                taskName,
                $"AddSubscriberMessage: Count={scanSubscribeList.Count}",
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

    private async void ButtonReactiveTimeout(int timeDelay)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"ButtonReactiveTimeout triggered",
            settingLevel,
            MessageLevel.Information
        );

        CancellationTokenSource cancellationTokenSource = new();

        try
        {
            var taskDelay = Task.Delay(timeDelay, cancellationTokenSource.Token);
            isConfirmReply = false;

            while (true)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"ButtonReactiveTimeout: isScanning={IsScanning}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (!IsScanning || isConfirmReply)
                {
                    isConfirmReply = false;
                    break;
                }

                if (IsScanning && taskDelay.IsCompleted)
                {
                    IsScanning = false;
                    commonLib?.DisplayConsole(
                        taskName,
                        $"ButtonReactiveTimeout: IsScanning={IsScanning}",
                        settingLevel,
                        MessageLevel.Trace
                    );
                    break;
                }

                await Task.Delay(100);
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

        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        await Task.CompletedTask;
    }
}
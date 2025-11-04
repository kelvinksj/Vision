using System.Runtime.Serialization;
using System.Text.Json;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.RCS;
using TaskManagerWeb.Components.Service.Mattel;

namespace TaskManagerWeb.Components.Service.RCS;
public class RCSService : IHostedService, IDisposable
{
    private string taskName = "RCSService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_RCSService;
    private bool disposed = false;
    private CommonLib? commonLib;
    private HttpClient? httpClient;
    private MattelService? mattelService;
    private PeriodicInvoker? periodicInvoker1;
    private PeriodicInvoker? periodicInvoker2;
    private bool gotMapInfo = false;
    private string mapJsonUrl = string.Empty;
    private RCSModel.MapElement mapJsonData = null!;
    private RCSModel.NodeList nodeList = new();
    private RCSModel.ShelfList shelfList = new();
    private int mapId = 0;

    public RCSService(
        CommonLib CommonLib,
        HttpClient HttpClient,
        MattelService MattelService
    )
    {
        CommonLib.DisplayConsole(
            taskName,
            $"RCSService triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            commonLib = CommonLib;
            httpClient = HttpClient;
            mattelService = MattelService;
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
            await Task.Delay(0, cancellationToken);

            periodicInvoker1 = new PeriodicInvoker(InvokeAsyncWrapper, 200);
            periodicInvoker1.StartPeriodicInvoke(RCSThread1);

            periodicInvoker2 = new PeriodicInvoker(InvokeAsyncWrapper, 1000);
            periodicInvoker2.StartPeriodicInvoke(RCSThread2);
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

        return;
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
            await Task.Delay(0, cancellationToken);

            try { await periodicInvoker1!.DisposeAsync(); } catch { }
            try { await periodicInvoker2!.DisposeAsync(); } catch { }
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

        return;
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
    ~RCSService()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
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

    private async Task RCSThread1(CancellationToken CancellationToken)
    // Http to TaskManager
    {
        try
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                if (!gotMapInfo)
                    await GetMapToken(CancellationToken);

                if (gotMapInfo && mapJsonData == null)
                    await GetMapData(CancellationToken);
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

    private async Task GetMapToken(CancellationToken CancellationToken)
    {
        string httpName = "GetMapToken";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        while (!CancellationToken.IsCancellationRequested)
        {
            try
            {
                RCSModel.RequestTopology httpCommand = new()
                {
                    AreaId = mapId
                };

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(httpCommand)}",
                    settingLevel,
                    MessageLevel.Trace
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        RCSModel.RCSHttp.ApiRequestMap,
                        httpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    commonLib?.DisplayConsole(
                        taskName,
                        $"{httpName}: data={commonLib?.JsonSerialize(result.Content)}",
                        settingLevel,
                        MessageLevel.Disable
                    );

                    RCSModel.ResponseTopology? data
                        = await result.Content.ReadFromJsonAsync<RCSModel.ResponseTopology>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: MapZipUrl={data.Data.MapZipUrl}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        if (data.Code == 1000
                            && data.Data.MapId == mapId
                            && data.Data.MapZipUrl != null
                            && data.Data.MapZipUrl != string.Empty
                        )
                        {
                            gotMapInfo = true;
                            mapJsonUrl = data.Data.MapJsonUrl;
                            mapJsonData = null!;
                            return;
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} http retry",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Trace
                        );
                    }
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

            await Task.Delay(intervalTime, CancellationToken);
            commonLib?.DisplayConsole(
                taskName,
                $"{httpName}: http intervalTime={intervalTime}",
                settingLevel,
                MessageLevel.Trace
            );

            if (intervalTime < finalTime)
                intervalTime += (int)(intervalTime * 0.2);
        }
    }

    private async Task GetMapData(CancellationToken CancellationToken)
    {
        string httpName = "GetMapData";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        while (!CancellationToken.IsCancellationRequested)
        {
            try
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: mapJsonUrl={mapJsonUrl}",
                    settingLevel,
                    MessageLevel.Trace
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result = await httpClient?.GetAsync(mapJsonUrl)!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    string reponse = await result.Content.ReadAsStringAsync();
                    RCSModel.MapElement? data = JsonSerializer.Deserialize<RCSModel.MapElement>(reponse);

                    if (reponse != null && data != null)
                    {
                        nodeList = new();

                        using (var doc = JsonDocument.Parse(reponse))
                        {
                            var root = doc.RootElement;

                            foreach (var item in root.GetProperty("nodeArr").EnumerateArray())
                            {
                                List<object> extraTypes = new List<object>();
                                foreach (var var7 in item[7].EnumerateArray())
                                {
                                    extraTypes.Add(var7.GetRawText());
                                }

                                nodeList.List.Add(new()
                                {
                                    X = item[0].GetInt32(),
                                    Y = item[1].GetInt32(),
                                    Type = item[2].GetInt32(),
                                    Content = item[3].GetString() ?? string.Empty,
                                    Name = item[4].GetString() ?? string.Empty,
                                    Isturn = item[5].GetInt32() == 1,
                                    ShelfIsTurn = item[6].GetInt32() == 1,
                                    ExtraTypes = extraTypes
                                });
                            }
                        }

                        mattelService?.SetRCSNodeList(nodeList);
                        mapJsonData = data;

                        var xMin = nodeList.List.Min(
                            n => n.X
                        );
                        var xMax = nodeList.List.Max(
                            n => n.X
                        );
                        var yMin = nodeList.List.Min(
                            n => n.Y
                        );
                        var yMax = nodeList.List.Max(
                            n => n.Y
                        );
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}",
                            settingLevel,
                            MessageLevel.Trace
                        );

                        return;
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: reponse=, data=null",
                            settingLevel,
                            MessageLevel.Trace
                        );
                    }
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

            await Task.Delay(intervalTime, CancellationToken);
            commonLib?.DisplayConsole(
                taskName,
                $"{httpName}: http intervalTime={intervalTime}",
                settingLevel,
                MessageLevel.Trace
            );

            if (intervalTime < finalTime)
                intervalTime += (int)(intervalTime * 0.2);
        }
    }

    private async Task RCSThread2(CancellationToken CancellationToken)
    // Http to TaskManager
    {
        try
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                if (gotMapInfo && mapJsonData != null)
                    await GetShelf(CancellationToken);
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

    private async Task GetShelf(CancellationToken CancellationToken)
    {
        string httpName = "GetShelf";
        commonLib?.DisplayConsole(
            taskName,
            $"{httpName} triggered",
            settingLevel,
            MessageLevel.Information
        );

        int intervalTime = 2000;
        int finalTime = 5000;
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        while (!CancellationToken.IsCancellationRequested)
        {
            try
            {
                RCSModel.RequestShelf httpCommand = new()
                {
                    AreaId = mapId.ToString(),
                    ShelfNums = string.Empty
                };

                commonLib?.DisplayConsole(
                    taskName,
                    $"{httpName}: command={commonLib?.JsonSerialize(httpCommand)}",
                    settingLevel,
                    MessageLevel.Disable
                );

                using CancellationTokenSource cts
                    = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                HttpResponseMessage result
                    = await httpClient?.PostAsJsonAsync(
                        RCSModel.RCSHttp.ApiRequestShelf,
                        httpCommand,
                        options,
                        cts.Token
                    )!;

                if (result?.IsSuccessStatusCode ?? false)
                {
                    RCSModel.ResponseShelf? data
                        = await result.Content.ReadFromJsonAsync<RCSModel.ResponseShelf>(
                            cancellationToken: CancellationToken
                        );

                    if (data != null)
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data={commonLib.JsonSerialize(data)}",
                            settingLevel,
                            MessageLevel.Disable
                        );

                        if (data.Code == 1000)
                        {
                            shelfList = new();
                            if (data.Data.Count > 0)
                            {
                                foreach (var item in data.Data)
                                {
                                    RCSModel.ShelfData newData = new();
                                    commonLib?.CopyProperties(item, newData);
                                    shelfList.List.Add(newData);
                                    var findLocation = nodeList.List.FirstOrDefault(
                                        n => n.Content == newData.ShelfCurrStation
                                    );

                                    if (findLocation != null)
                                    {
                                        findLocation.ShelfData = newData;
                                    }
                                }
                            }

                            mattelService?.SetRCSShelfList(shelfList);
                            mattelService?.SetRCSNodeList(nodeList);
                            return;
                        }
                        else
                        {
                            commonLib?.DisplayConsole(
                                taskName,
                                $"{httpName} http retry",
                                settingLevel,
                                MessageLevel.Information
                            );
                        }
                    }
                    else
                    {
                        commonLib?.DisplayConsole(
                            taskName,
                            $"{httpName}: data=null",
                            settingLevel,
                            MessageLevel.Trace
                        );
                    }
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

            await Task.Delay(intervalTime, CancellationToken);
            commonLib?.DisplayConsole(
                taskName,
                $"{httpName}: http intervalTime={intervalTime}",
                settingLevel,
                MessageLevel.Trace
            );

            if (intervalTime < finalTime)
                intervalTime += (int)(intervalTime * 0.2);
        }
    }

    public void RequestMap(int MapId)
    {
        mapId = MapId;
        gotMapInfo = false;
        mapJsonData = null!;
    }
}
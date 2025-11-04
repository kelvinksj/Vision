using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.KeepAlive;
public class KeepAliveService : IHostedService, IDisposable
{
    private readonly string taskName = "KeepAliveService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ServAliveServ;
    private CommonLib commonLib = new CommonLib();
    private readonly IServiceProvider? serviceProvider;
    private HttpClient? httpClient;
    private NavigationManager? navigationManager;
    private Timer? timer;
    private int activePages = 0;
    private readonly ConcurrentDictionary<string, Guid> pageGuids = new();

    public KeepAliveService(IServiceProvider ServiceProvider)
    {
        serviceProvider = ServiceProvider;
    }

    public Guid PageOpened(
        IHttpClientFactory HttpClientFactory,
        NavigationManager NavigationManager,
        string CurrentUserName
    )
    {
        Guid pageGuid = Guid.NewGuid();

        if (CurrentUserName?.Length > 0)
        {
            if (pageGuids.TryGetValue(CurrentUserName, out Guid oldGuid))
            {
                pageGuid = oldGuid;
                commonLib?.DisplayConsole(
                    taskName,
                    $"PageOpened: CurrentUserName={CurrentUserName}, oldGuid={oldGuid}",
                    settingLevel,
                    MessageLevel.Trace
                );

            }
            else
            {
                pageGuids[CurrentUserName] = pageGuid;
                commonLib?.DisplayConsole(
                    taskName,
                    $"PageOpened: CurrentUserName={CurrentUserName}, pageGuid={pageGuid}",
                    settingLevel,
                    MessageLevel.Trace
                );

            }
        }

        if (Interlocked.Increment(ref activePages) == 1)
        {
            httpClient = HttpClientFactory.CreateClient("MyClient");
            navigationManager = NavigationManager;
            StartAsync(default);
        }

        commonLib?.DisplayConsole(
            taskName,
            $"PageOpened: pageGuid={pageGuid}",
            settingLevel,
            MessageLevel.Trace
        );
        commonLib?.DisplayConsole(
            taskName,
            $"PageOpened: activePages={activePages}",
            settingLevel,
            MessageLevel.Trace
        );

        return pageGuid;
    }

    public Task PageClosed(string CurrentUserName)
    {
        pageGuids.TryRemove(CurrentUserName, out _);

        if (Interlocked.Decrement(ref activePages) == 0)
        {
            StopAsync(default);
        }

        commonLib?.DisplayConsole(
            taskName,
            $"PageOpened: activePages={activePages}",
            settingLevel,
            MessageLevel.Trace
        );

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken CancellationToken)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"StartAsync trigged",
            settingLevel,
            MessageLevel.Information
        );

        timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private async void DoWork(object? State)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"DoWork trigged",
            settingLevel,
            MessageLevel.Information
        );

        if (serviceProvider != null)
        {
            using IServiceScope? scope = serviceProvider.CreateScope();

            try
            {
                if (httpClient != null && navigationManager != null)
                {
                    string currentUrl = navigationManager.Uri;

                    commonLib?.DisplayConsole(
                        taskName,
                        $"DoWork: currentUrl={currentUrl}",
                        settingLevel,
                        MessageLevel.Trace
                    );

                    Uri uri = new Uri(currentUrl);
                    string schemaAndHostAndPort = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                    string urlKeepAlive = $"{schemaAndHostAndPort}/Api/KeepAlive";

                    commonLib?.DisplayConsole(
                        taskName,
                        $"DoWork: schemaAndHostAndPort={schemaAndHostAndPort}",
                        settingLevel,
                        MessageLevel.Trace
                    );
                    commonLib?.DisplayConsole(
                        taskName,
                        $"DoWork: urlKeepAlive={urlKeepAlive}",
                        settingLevel,
                        MessageLevel.Trace
                    );
                    HttpResponseMessage? response = await httpClient!.GetAsync(urlKeepAlive);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    commonLib?.DisplayConsole(
                        taskName,
                        $"DoWork: responseBody={responseBody}",
                        settingLevel,
                        MessageLevel.Trace
                    );
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
    }

    public Task StopAsync(CancellationToken CancellationToken)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"StopAsync trigged",
            settingLevel,
            MessageLevel.Information
        );

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"Dispose trigged",
            settingLevel,
            MessageLevel.Information
        );

        timer?.Dispose();
    }
}
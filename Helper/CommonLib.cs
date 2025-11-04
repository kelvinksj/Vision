using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;

namespace TaskManagerWeb.Components.Helper;
/// <summary>
/// Specifies the severity level of a log message.
/// </summary>
public enum MessageLevel
{
    /// <summary>
    /// Disable logging.
    /// </summary>
    Disable,

    /// <summary>
    /// No logging.
    /// </summary>
    None,

    /// <summary>
    /// Logs that contain the most detailed messages.
    /// </summary>
    Trace,

    /// <summary>
    /// Logs that are used for interactive investigation during development.
    /// </summary>
    Debug,

    /// <summary>
    /// Logs that track the general flow of the application.
    /// </summary>
    Information,

    /// <summary>
    /// Logs that highlight an abnormal or unexpected event in the application flow.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs that highlight when the current flow of execution is stopped due to a failure.
    /// </summary>
    Error,

    /// <summary>
    /// Logs that describe an unrecoverable application or system crash.
    /// </summary>
    Critical,

    /// <summary>
    /// Logs that always show.
    /// </summary>
    Always
}

/// <summary>
/// Common library that can be use in this project .
/// </summary>
public class CommonLib
{
    private readonly string taskName = "CommonLib";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_HelpCom;
    private IDataProtectionProvider? dataProtectionProvider { get; set; }
    private IDataProtector? protector;
    private readonly JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

    public CommonLib()
    {
        try
        {
            dataProtectionProvider = DataProtectionProvider.Create("CommonLib");
            protector = dataProtectionProvider.CreateProtector("kostuS?_sTlC*ma1Lx0y");
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    /// <summary>
    /// Returns the current date and time as a string formatted in "yyyyMMddHHmmss".
    /// </summary>
    /// <returns>A string representing the current date and time.</returns>
    public string GetDateTime()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    /// <summary>
    /// Outputs a formatted message to the console if the setting message level
    /// is less than or equal to the specified message level.
    /// </summary>
    /// <param name="TaskName">The name of the task to be displayed.</param>
    /// <param name="Message">The message to be displayed.</param>
    /// <param name="SettingLevel">The setting level of the message.</param>
    /// <param name="MessageLevel">The level against which the setting level is checked.</param>
    public void DisplayConsole(
        string TaskName,
        string Message,
        MessageLevel SettingLevel,
        MessageLevel MessageLevel
    )
    {
        try
        {
            if (SettingLevel <= MessageLevel)
                Console.WriteLine($"{TaskName}-{GetDateTime()}: {Message}");
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    public void DisplayConsole(
        string TaskName,
        Exception ex,
        MessageLevel SettingLevel,
        MessageLevel MessageLevel
    )
    {
        if (SettingLevel <= MessageLevel)
        {
            var st = new StackTrace(ex, true);
            var header = $"{TaskName}-{GetDateTime()}:";
            var total = st.FrameCount;

            for (int i = 0; i < total; i++)
            {
                var frame = st.GetFrame(i);
                var fileName = frame!.GetFileName();
                var method = frame.GetMethod();
                var line = frame.GetFileLineNumber();
                var column = frame.GetFileColumnNumber();
                var gg = frame.ToString();

                var msg = $"{header} {method} err[{i + 1}/{total}]:";
                msg += $"[{ex.HResult}][{line},{column}]{ex.Message}";
                Console.WriteLine(msg);
            }

            Console.WriteLine($"{header} {ex}");
        }
    }

    public string JsonSerialize(
        object DataStructure,
        bool IsEncrypted = false
    )
    {
        string jsonString = string.Empty;
        try
        {
            jsonString = JsonSerializer.Serialize(DataStructure, options);

            if (IsEncrypted)
            {
                string encryptData = EncryptData(jsonString);
                return encryptData;
            }
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return jsonString;
    }

    public void JsonSerializeFile(
        object DataStructure,
        string Filename,
        bool IsCreateFile = false,
        bool IsEncrypted = false
    )
    {
        try
        {
            string jsonString = JsonSerialize(DataStructure, IsEncrypted);

            if (!File.Exists(Filename) || IsCreateFile)
            {
                File.WriteAllText(Filename, jsonString);
            }
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    public T? JsonDeserializeFile<T>(
        string Filename,
        bool IsEncrypted = false
    )
    {
        try
        {
            string jsonString = File.ReadAllText(Filename);

            if (IsEncrypted)
            {
                string originalData = DecryptData(jsonString);
                return JsonSerializer.Deserialize<T>(originalData);
            }
            else
            {
                DisplayConsole(
                    taskName,
                    $"JsonDeserializeFile jsonString={jsonString}",
                    settingLevel,
                    MessageLevel.Trace
                );
                return JsonSerializer.Deserialize<T>(jsonString);
            }

        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            return default;
        }
    }

    public string EncryptData(string OriginalData)
    {
        try
        {
            return protector!.Protect(OriginalData);
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            return string.Empty;
        }
    }

    public string DecryptData(string EncryptedData)
    {
        try
        {
            return protector!.Unprotect(EncryptedData);
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            return string.Empty;
        }
    }

    public string GetPageName(
        string Url,
        int NumberOfParams
    )
    {
        string pageName = string.Empty;

        try
        {
            pageName = Url.Split('/')
                          .Where(x => !string.IsNullOrWhiteSpace(x))
                          .Reverse()
                          .Skip(NumberOfParams)
                          .FirstOrDefault() ?? string.Empty;
        }
        catch (Exception ex)
        {
            DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            return string.Empty;
        }

        return pageName;
    }

    public void CopyProperties(object Source, object Destination)
    {
        var sourceProperties = Source.GetType().GetProperties();
        var destinationProperties = Destination.GetType().GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            try
            {
                var destinationProperty = destinationProperties.FirstOrDefault(
                    p => p.Name == sourceProperty.Name
                    && p.PropertyType == sourceProperty.PropertyType
                );

                if (destinationProperty != null && destinationProperty.CanWrite)
                {
                    destinationProperty.SetValue(Destination, sourceProperty.GetValue(Source));
                }
            }
            catch (Exception ex)
            {
                DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }
    }
}

public static class CommonMethods
{
    private static CommonLib commonLib = new();

    // Class to hold all Parameters
    public class MonitorParameters
    {
        public bool IsStart { get; set; } = false;
        public bool IsAlive { get; set; } = false;
        public int IntervalTime { get; set; } = 0;
        public int AddTimeout { get; set; } = 1000;
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public Func<Task<bool>>? OnTimeout { get; set; }
        public string TaskName { get; set; } = "Not assign";
        public MessageLevel MessageLevel { get; set; } = MessageLevel.Error;
    }

    public static async void MonitorSubscribe(MonitorParameters Parameters)
    {
        commonLib.DisplayConsole(
            Parameters.TaskName,
            $"{Parameters.TaskName}: MonitorSubscribe triggered",
            Parameters.MessageLevel,
            MessageLevel.Information
        );

        try
        {
            while (!Parameters.CancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Parameters.IsStart)
                {
                    commonLib.DisplayConsole(
                        Parameters.TaskName,
                        $"{Parameters.TaskName}: MonitorSubscribe monitor started",
                        Parameters.MessageLevel,
                        MessageLevel.Information
                    );
                    await RunInnerLoop(Parameters);
                }

                await Task.Delay(100);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            commonLib.DisplayConsole(
                $"{Parameters.TaskName}: MonitorSubscribe:",
                ex,
                Parameters.MessageLevel,
                MessageLevel.Error
            );
        }
        finally
        {
            Parameters.CancellationTokenSource.Dispose();
        }
    }

    private static async Task RunInnerLoop(MonitorParameters Parameters)
    {
        try
        {
            var newTime = Parameters.IntervalTime + Parameters.AddTimeout;
            var taskTimeout = Task.Delay(
                newTime,
                Parameters.CancellationTokenSource.Token
            );

            while (!Parameters.CancellationTokenSource.Token.IsCancellationRequested
                && !taskTimeout.IsCompleted
            )
            {
                if (!Parameters.IsStart)
                {
                    commonLib.DisplayConsole(
                        Parameters.TaskName,
                        $"{Parameters.TaskName}: MonitorSubscribe monitor stopped",
                        Parameters.MessageLevel,
                        MessageLevel.Information
                    );
                    return;
                }

                if (Parameters.IsAlive)
                {
                    taskTimeout = Task.Delay(
                        newTime,
                        Parameters.CancellationTokenSource.Token
                    );
                    Parameters.IsAlive = false;
                }

                await Task.Delay(100);
                await Task.Yield();
            }

            if (!Parameters.CancellationTokenSource.Token.IsCancellationRequested
                && !Parameters.IsAlive
                && Parameters.OnTimeout != null
            )
            {
                commonLib.DisplayConsole(
                    Parameters.TaskName,
                    $"{Parameters.TaskName}: MonitorSubscribe detected not alive",
                    Parameters.MessageLevel,
                    MessageLevel.Information
                );
                _ = Parameters.OnTimeout();
            }
        }
        catch (Exception ex)
        {
            commonLib.DisplayConsole(
                $"{Parameters.TaskName}: RunInnerLoop:",
                ex,
                Parameters.MessageLevel,
                MessageLevel.Error
            );
        }
    }
}

public static class ExtensionMethods
{
    public static T DeepClone<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json) ?? default!;
    }
}

public static class EnumMapper
{
    public static TTarget MapEnum<TSource, TTarget>(TSource source) where TSource : Enum where TTarget : Enum
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "Source enum value cannot be null.");

        if (!typeof(TSource).IsEnum)
            throw new ArgumentException($"{typeof(TSource).Name} is not an enum type.");

        if (!typeof(TTarget).IsEnum)
            throw new ArgumentException($"{typeof(TTarget).Name} is not an enum type.");

        try
        {
            if (!Enum.IsDefined(typeof(TTarget), source.ToString()))
                throw new ArgumentOutOfRangeException(nameof(source), source, null);

            return (TTarget)Enum.Parse(typeof(TTarget), source.ToString());
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException("Failed to map enum value.", ex);
        }
    }
}

public static class JsInteropHelper
{
    public static async Task FocusMyHeader(IJSRuntime jsRuntime, string ID = "MyHeader")
    {
        string jsCommand = $"document.getElementById('{ID}').focus();";
        await jsRuntime.InvokeVoidAsync("eval", jsCommand);
    }

    public static async Task RemoveAllRangesAsync(IJSRuntime jsRuntime)
    {
        string jsCommand = "document.getSelection().removeAllRanges();";
        await jsRuntime.InvokeVoidAsync("eval", jsCommand);
    }

    public static async Task PopMsgBtnBlur(IJSRuntime jsRuntime, string ClassName = "PopMsgBtn")
    {
        string jsCommand = $"const elements = document.getElementsByClassName('{ClassName}'); ";
        jsCommand += "for (let element of elements) { element.blur(); }";
        await jsRuntime.InvokeVoidAsync("eval", jsCommand);
    }

    public static async Task PrintDialog(IJSRuntime jsRuntime)
    {
        string jsCommand = "window.print();";
        await jsRuntime.InvokeVoidAsync("eval", jsCommand);
    }

    public static async Task PrintSpecificArea(IJSRuntime jsRuntime, string PrintName = "PrintThis")
    {
        string jsCommand = $"var printContents = document.getElementById('{PrintName}').innerHTML; ";
        jsCommand += "document.body.innerHTML = printContents; ";
        jsCommand += "window.print(); ";
        jsCommand += "window.location.reload();";
        await jsRuntime.InvokeVoidAsync("eval", jsCommand);
    }
}
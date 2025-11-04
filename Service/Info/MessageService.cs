using Microsoft.JSInterop;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.Info;
public class MessageService
{
    private static CommonLib commonLib = new CommonLib();
    private static string taskName = "MessageService";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_MessageService;

    public MessageService(CommonLib CommonLib)
    {
        commonLib = CommonLib;
    }

    [JSInvokable("MessageFromJS")]
    public static void MessageFromJS(string Message)
    {
        commonLib.DisplayConsole(
            taskName,
            $"MessageFromJS: {Message}",
            settingLevel,
            MessageLevel.Trace
        );
    }
}
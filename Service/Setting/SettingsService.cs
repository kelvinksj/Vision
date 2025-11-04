using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Setting;
using TaskManagerWeb.Components.Models.RCS;
using TaskManagerWeb.Components.Service.RCS;

namespace TaskManagerWeb.Components.Service.Setting;
public class SettingsService
{
    private string taskName = "SettingsService";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_SettingsService;
    private CommonLib commonLib;
    private RCSService rCSService;
    private string fileName = "wwwroot/json/Settings.json";
    private bool isEncrypte = false;
    private SettingsInfo info = new SettingsInfo();

    public SettingsService(
        CommonLib CommonLib,
        RCSService RCSService
    )
    {
        commonLib = CommonLib;
        rCSService = RCSService;

        try
        {
            info = new()
            {
                MyNetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.120",
                    Port = "20000"
                },
                TaskManagerNetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.124",
                    Port = "20040"
                },
                ContainerNetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.125",
                    Port = "20060"
                },
                Container2NetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.125",
                    Port = "20051"
                },
                ScanNetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.126",
                    Port = "20070"
                },
                RCSNetData = new()
                {
                    IsHttps = false,
                    IP = "70.70.70.2",
                    Port = "7000"
                }
            };

            commonLib.JsonSerializeFile(
                info,
                fileName,
                IsEncrypted: isEncrypte
            );
            info = commonLib.JsonDeserializeFile<SettingsInfo>(
                fileName, isEncrypte) ?? new SettingsInfo();
            UpdateInfo(info);
            rCSService.RequestMap(info.RCSData.MapId);
        }
        catch (Exception ex)
        {
            commonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    public SettingsInfo GetInfo()
    {
        return info.DeepClone();
    }

    public SettingsInfo GetDefault()
    {
        SettingsInfo defaultInfo = new()
        {
            MyNetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.120",
                Port = "20000"
            },
            TaskManagerNetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.124",
                Port = "20040"
            },
            ContainerNetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.125",
                Port = "20060"
            },
            Container2NetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.125",
                Port = "20050"
            },
            ScanNetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.126",
                Port = "20070"
            },
            RCSNetData = new()
            {
                IsHttps = false,
                IP = "70.70.70.2",
                Port = "7000"
            }
        };

        return defaultInfo;
    }

    public void UpdateInfo(SettingsInfo Info)
    {
        try
        {
            info = Info;
            HttpParameters.SetMyIP(
                info.MyNetData.IsHttps,
                info.MyNetData.IP,
                info.MyNetData.Port
            );
            HttpParameters.SetServiceIP(
                info.TaskManagerNetData.IsHttps,
                info.TaskManagerNetData.IP,
                info.TaskManagerNetData.Port
            );
            HttpParameters.SetContainerIP(
                info.ContainerNetData.IsHttps,
                info.ContainerNetData.IP,
                info.ContainerNetData.Port
            );
            HttpParameters.SetContainer2IP(
                info.Container2NetData.IsHttps,
                info.Container2NetData.IP,
                info.Container2NetData.Port
            );
            HttpParameters.SetScanIP(
                info.ScanNetData.IsHttps,
                info.ScanNetData.IP,
                info.ScanNetData.Port
            );
            RCSModel.RCSHttp.SetRCSIP(
                info.RCSNetData.IsHttps,
                info.RCSNetData.IP,
                info.RCSNetData.Port
            );
            commonLib.JsonSerializeFile(
                info,
                fileName,
                true,
                isEncrypte
            );
            rCSService.RequestMap(info.RCSData.MapId);

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: MyEndpoint="
                + $"{HttpParameters.MyEndpoint}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uicommand="
                + $"{HttpParameters.ServiceEndpoint_uicommand}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uicancel="
                + $"{HttpParameters.ServiceEndpoint_uicancel}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uisubscribe="
                + $"{HttpParameters.ServiceEndpoint_uisubscribe}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uiamrsubscribe="
                + $"{HttpParameters.ServiceEndpoint_uiamrsubscribe}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uiamrcommand="
                + $"{HttpParameters.ServiceEndpoint_uiamrcommand}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ServiceEndpoint_uitablecommand="
                + $"{HttpParameters.ServiceEndpoint_uitablecommand}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ContainerEndpoint_locationSubPub="
                + $"{HttpParameters.ContainerEndpoint_locationSubPub}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ContainerEndpoint_cancelSubscription="
                + $"{HttpParameters.ContainerEndpoint_cancelSubscription}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ContainerEndpoint_uicommand="
                + $"{HttpParameters.ContainerEndpoint_uicommand}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ScanEndpoint_scan="
                + $"{HttpParameters.ScanEndpoint_scan}",
                settingLevel,
                MessageLevel.Information
            );
            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ScanEndpoint_scanprogress="
                + $"{HttpParameters.ScanEndpoint_scanprogress}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: ApiRequestMap="
                + $"{RCSModel.RCSHttp.ApiRequestMap}",
                settingLevel,
                MessageLevel.Information
            );

            commonLib.DisplayConsole(
                taskName,
                $"UpdateInfo: MapId="
                + $"{info.RCSData.MapId}",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            commonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }
}
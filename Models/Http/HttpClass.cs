using System.Net;
using System.Text.Json.Serialization;

namespace TaskManagerWeb.Components.Models.Http;
public class HttpClass
{
    // Response ---------------------------------------------------------
    public class GenericReply
    {
        [JsonPropertyName("code")]
        required public int Code { get; set; } = 1000;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = "succeed";

        [JsonPropertyName("responseId")]
        required public string ResponseId { get; set; } = "";
    }

    // Request ---------------------------------------------------------
    public class GenericRequest
    {
        [JsonPropertyName("id")]
        public required string ID { get; set; } = string.Empty; // the ID of the UI device

        [JsonPropertyName("callId")]
        public required string CallID { get; set; } = string.Empty;
    }

    public class HttpUIResponse : GenericReply
    {

    }

    public class CheckInUpdate : GenericRequest
    {
        [JsonPropertyName("commandID")]
        public string CommandID { get; set; } = string.Empty;

        [JsonPropertyName("statusList")]
        public List<CheckInStatus> StatusList { get; set; } = [];
    }

    public class CheckInStatus
    {
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class CommandClass
    {
        [JsonPropertyName("commandID")]
        required public string CommandID { get; set; } = string.Empty;

        [JsonIgnore]
        public Type Type { get; set; } = null!;
    }

    public class TableCommandClass : CommandClass
    {
        [JsonPropertyName("tableCode")]
        public string TableCode { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("moveToPosition")]
        // Use for AMR move command
        public string MoveToPosition { get; set; } = string.Empty;
    }

    public class HttpTableCommand : GenericRequest
    {
        [JsonPropertyName("command")]
        required public TableCommandClass Command { get; set; }
    }

    public class HttpTableResponse : GenericReply
    {

    }

    public class AMRCommandClass : CommandClass
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("serialNo")]
        public string SerialNo { get; set; } = string.Empty;

        [JsonPropertyName("moveToPosition")]
        // Use for AMR move command
        public string MoveToPosition { get; set; } = string.Empty;
    }

    public class HttpAMRCommand : GenericRequest
    {
        [JsonPropertyName("command")]
        required public AMRCommandClass Command { get; set; }
    }

    public class UICommandClass : CommandClass
    {
        [JsonPropertyName("loadType")]
        public string LoadType { get; set; } = string.Empty; // containertype

        [JsonPropertyName("locationID")]
        public string LocationID { get; set; } = string.Empty;

        [JsonPropertyName("kitID")]
        public string KitID { get; set; } = string.Empty;

        [JsonPropertyName("sKUID")]
        public string SKUID { get; set; } = string.Empty;

        [JsonPropertyName("behaviour")]
        public string Behaviour { get; set; } = string.Empty;
    }

    public class HttpUICommand : GenericRequest
    {
        [JsonPropertyName("command")]
        required public UICommandClass Command { get; set; }
    }

    public class HttpAMRResponse : GenericReply
    {

    }

    public static class UICommand
    {
        public const string Cancel = "Cancel";
        public const string CheckOut = "CheckOut";
        public const string Clear = "Clear";
        public const string ContainerUpdate = "ContainerUpdate";
        public const string Last = "Last";
        public const string Load = "Load";
        public const string LocationSetting = "LocationSetting";
        public const string Maintenance = "Maintenance";
        public const string Move = "Move";
        public const string Pause = "Pause";
        public const string Quarantine = "Quarantine";
        public const string Restore = "Restore";
        public const string Request = "Request";
        public const string Suspend = "Suspend";
        public const string Unknown = "Unknown";
    }

    public class OrderLoadType
    {
        public const string Gaylord = "Gaylord";
        public const string FG = "FG";
        public const string NA = "NA";
        public const string SKU = "SKU";
    }

    public class StatusType
    {
        public const string Empty = "Empty";
        public const string Full = "Full";
        public const string Loaded = "Loaded";
        public const string LoadOut = "LoadOut";
    }

    public class ConditionType
    {
        public const string Normal = "Normal";
        public const string Quarantine = "Quarantine";
        public const string CheckOut = "CheckOut";
    }

    public class BehaviourType
    {
        public const string Pooling = "Pooling";
        public const string OnChanged = "OnChanged";
        public const string Query = "Query";
        public const string Maintenance = "Maintenance";
        public const string CheckOut = "CheckOut";
    }

    public class LocationStatusUpdateSubscriber : GenericRequest
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("requestBehaviour")]
        public RequestBehaviour RequestBehaviour { get; set; } = new();

        [JsonPropertyName("location")]
        public List<string> Location { get; set; } = [];

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = new();
    }

    public class ContainerStatusUpdateSubscriber : GenericRequest
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("requestBehaviour")]
        public RequestBehaviour RequestBehaviour { get; set; } = new();

        [JsonPropertyName("containerId")]
        public List<string> ContainerId { get; set; } = [];
    }

    public class UrlData
    {
        [JsonPropertyName("isHttps")]
        public bool IsHttps { get; set; } = false;

        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("port")]
        public string Port { get; set; } = string.Empty;

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; } = string.Empty;
    }

    public class RequestBehaviour
    {
        [JsonPropertyName("behaviour")]
        public string Behaviour { get; set; } = string.Empty;

        [JsonPropertyName("intervalTime")]
        public int IntervalTime { get; set; } = 0; // milliseconds

        [JsonPropertyName("subscriptionTime")]
        public int SubscriptionTime { get; set; } = 0; // minutes

        [JsonPropertyName("filter")]
        public string Filter { get; set; } = string.Empty;
    }

    public class StatusUpdateResponse : GenericReply
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ContainerData> Data { get; set; } = [];
    }

    public class SubscriptionUpdate : GenericRequest
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ContainerData> Data { get; set; } = [];

        [JsonPropertyName("isLast")]
        public bool IsLast { get; set; } = false;
    }

    public class ContainerData
    {
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("containerId")]
        public string ContainerId { get; set; } = string.Empty;

        [JsonPropertyName("loadType")]
        public string LoadType { get; set; } = string.Empty;

        [JsonPropertyName("kitId")]
        public string KitId { get; set; } = string.Empty;

        [JsonPropertyName("skuId")]
        public string SkuId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("indexDateTime")]
        public string IndexDateTime { get; set; } = string.Empty;

        [JsonPropertyName("lastMove")] // "yyyyMMddHHmmss"
        public string LastMove { get; set; } = string.Empty;

        [JsonPropertyName("lastLocation")]
        public string LastLocation { get; set; } = string.Empty;

        [JsonPropertyName("isLock")]
        public bool IsLock { get; set; } = false;

        [JsonPropertyName("colorBG")]
        public string ColorBG { get; set; } = string.Empty;

        [JsonPropertyName("colorFG")]
        public string ColorFG { get; set; } = string.Empty;

        [JsonPropertyName("locCondition")]
        public string LocCondition { get; set; } = string.Empty;
    }

    public class CancelSubscription : GenericRequest
    {
        [JsonPropertyName("subscribeId")]
        public List<string> SubscribeId { get; set; } = [];
    }

    public class ContainerCommand : GenericRequest
    {
        [JsonPropertyName("commandID")]
        public string CommandID { get; set; } = string.Empty;

        [JsonPropertyName("containerUpdateData")]
        public ContainerData ContainerUpdateData { get; set; } = new();
    }

    public class SettingCommand : GenericRequest
    {
        [JsonPropertyName("commandID")]
        public string CommandID { get; set; } = string.Empty;

        [JsonPropertyName("locationParameter")]
        public List<LocationSetting> LocationParameter { get; set; } = [];
    }

    public class LocationSetting
    {
        [JsonPropertyName("locationID")]
        public string LocationID { get; set; } = string.Empty;

        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;
    }

    public class DisplayString
    {
        [JsonPropertyName("text")]
        // string text to be displayed
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("foreground")]
        public string Foreground { get; set; } = "#ffffff";

        [JsonPropertyName("background")]
        public string Background { get; set; } = "#000000";
    }

    public static class UIMessageStatus // version 1.1
    {
        public const string Open = "Open";
        // Being cancelled -> cannot cancel anymore
        public const string Cancelled = "Cancelled";
    }

    public class UIMessage
    // use for message box.
    {
        [JsonPropertyName("taskId")]
        public string TaskID { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public DisplayString Title { get; set; } = new();

        [JsonPropertyName("textList")]
        public List<DisplayString> TextList { get; set; } = [];

        [JsonPropertyName("status")]
        // Not being displayed.  Cancelled -> still on display because there is ongoing recovery task
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("areaID")]
        // which area = used for UI display filtering
        public string AreaID { get; set; } = string.Empty;

        [JsonPropertyName("locationID")]
        // this order is for which location
        public string LocationID { get; set; } = string.Empty;

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = new();
    }

    public class HttpUISubscribeCommand : GenericRequest
    // Subscribe request to TaskManager
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("requestBehaviour")]
        public RequestBehaviour RequestBehaviour { get; set; } = new();

        [JsonPropertyName("areaId")]
        public List<string> AreaId { get; set; } = [];
    }

    public class HttpUISubscribeResponse : GenericReply
    // Subscribe reply from TaskManager
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;
    }

    public class HttpUISubscriptionUpdate : GenericRequest
    // From TaskManager subscript update
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<UIMessage> Messages { get; set; } = [];

        [JsonPropertyName("isLast")]
        public bool IsLast { get; set; } = false;
    }

    public class HttpUICancelCommand : GenericRequest
    // use for message box.
    {
        [JsonPropertyName("taskID")]
        public string TaskID { get; set; } = string.Empty;

        [JsonPropertyName("commandID")]
        public string CommandID { get; set; } = string.Empty;
    }

    public class HttpUICancelResponse : GenericReply
    // use for message box.
    {

    }

    public class ScanData : GenericRequest
    //  From ScanManager scan data
    {
        [JsonPropertyName("scanId")]
        public string ScanId { get; set; } = string.Empty;

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = new();

        [JsonPropertyName("kitData")]
        public List<KitData> KitData { get; set; } = [];
    }

    public class ScanReply : GenericReply
    {
        [JsonPropertyName("scanId")]
        public string ScanId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; set; } = string.Empty;

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = new();
    }

    public class KitData
    // Scan use
    {
        [JsonPropertyName("kit")]
        public string Kit { get; set; } = string.Empty;

        [JsonPropertyName("partNoData")]
        public List<PartNoData> PartNoData { get; set; } = [];
    }

    public class PartNoData
    // Scan use
    {
        [JsonPropertyName("partNo")]
        public string PartNo { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
    }

    public class ScanRequest : GenericRequest
    // Request scan to ScanManager
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "One";

        [JsonPropertyName("format")]
        public string Format { get; set; } = "pdf";

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = new();
    }

    public class ScanUISubscribeCommand : GenericRequest
    // Subscribe request to ScanManager message
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("requestBehaviour")]
        public RequestBehaviour RequestBehaviour { get; set; } = new();

        [JsonPropertyName("areaId")]
        public List<string> AreaId { get; set; } = [];
    }

    public class ScanUISubscribeResponse : GenericReply
    // Subscribe reply from ScanManager message
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;
    }

    public class ScanUISubscriptionUpdate : GenericRequest
    // From ScanManager message subscript update
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<UIMessage> Messages { get; set; } = [];

        [JsonPropertyName("isLast")]
        public bool IsLast { get; set; } = false;
    }

    public class HttpAmrSubscribeCommand : GenericRequest
    // Subscribe request to TaskManager for AMR
    {
        [JsonPropertyName("urlData")]
        public UrlData UrlData { get; set; } = new();

        [JsonPropertyName("requestBehaviour")]
        public RequestBehaviour RequestBehaviour { get; set; } = new();

        [JsonPropertyName("aMR")]
        public List<string> AMR { get; set; } = [];
    }

    public class HttpAmrSubscribeResponse : GenericReply
    // Subscribe reply from TaskManager for AMR
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;
    }

    public class HttpAmrSubscriptionUpdate : GenericRequest
    // From TaskManager subscript update for AMR
    {
        [JsonPropertyName("subscribeId")]
        public string SubscribeId { get; set; } = string.Empty;

        [JsonPropertyName("aMR")]
        public List<AMR> AMR { get; set; } = [];

        [JsonPropertyName("isLast")]
        public bool IsLast { get; set; } = false;
    }

    public class AMR
    // use in HttpAmrSubscriptionUpdate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("serialNo")]
        public string SerialNo { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("batteryLevel")]
        public string BatteryLevel { get; set; } = string.Empty;
    }

    public class HttpAmrCancelCommand : GenericRequest
    // Cancel subscribe to TaskManager For AMR
    {
        [JsonPropertyName("taskID")]
        public string TaskID { get; set; } = string.Empty;

        [JsonPropertyName("commandID")]
        public string CommandID { get; set; } = string.Empty;
    }

    public class HttpAmrCancelResponse : GenericReply
    // From TaskManager reply to cancel for AMR
    {

    }
}
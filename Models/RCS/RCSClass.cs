using System.Text.Json.Serialization;
using TaskManagerWeb.Components.Models.Http;

namespace TaskManagerWeb.Components.Models.RCS;
/// <summary>
/// Http settings.
/// </summary>
public class RCSModel
{
    public static class RCSHttp
    {
        public static string RCSHeader { get; set; } = "http";
        public static string RCSIP { get; set; } = "192.168.10.21";
        public static string RCSPort { get; set; } = "7000";
        public static string ApiRequestMap { get; set; } =
            $"{RCSHeader}://{RCSIP}:{RCSPort}/ics/out/topologyList";
        public static string ApiRequestShelf { get; set; } =
            $"{RCSHeader}://{RCSIP}:{RCSPort}/ics/out/shelf/getShelfPosition";

        public static void SetRCSIP(bool IsHttps, string IP, string Port)
        {
            string header = IsHttps ? "https" : "http";
            RCSHeader = header;
            RCSIP = IP;
            RCSPort = Port;
            ApiRequestMap = $"{header}://{IP}:{Port}/ics/out/topologyList";
            ApiRequestShelf = $"{header}://{IP}:{Port}/ics/out/shelf/getShelfPosition";
        }
    }

    /// <summary>
    /// RCS API TopologyList request structure.
    /// </summary>
    public class RequestTopology
    {
        [JsonPropertyName("areaId")]
        public int AreaId { get; set; } = 1;
    }

    /// <summary>
    /// RCS API TopologyList response structure.
    /// </summary>
    public class ResponseTopology
    {
        [JsonPropertyName("code")]
        public int Code { get; set; } = 0;

        [JsonPropertyName("data")]
        public MapInfo Data { get; set; } = new();

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        public class MapInfo
        {
            [JsonPropertyName("mapZipUrl")]
            public string MapZipUrl { get; set; } = string.Empty;

            [JsonPropertyName("mapId")]
            public int MapId { get; set; } = 0;

            [JsonPropertyName("mapJsonUrl")]
            public string MapJsonUrl { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// JSON structure for "nodeArr" object from RCS compress.json.
    /// </summary>
    public class MapElement
    {
        [JsonPropertyName("nodeKeys")]
        public List<string> NodeKeys { get; set; } = [];

        [JsonPropertyName("lineKeys")]
        public List<string> LineKeys { get; set; } = [];

        [JsonPropertyName("nodeArr")]
        public List<List<Object>> NodeArr { get; set; } = [];

        [JsonPropertyName("lineArr")]
        public List<List<object>> LineArr { get; set; } = [];

        [JsonPropertyName("allPath")]
        public List<List<List<double>>> AllPath { get; set; } = [];

        [JsonPropertyName("chargeCoor")]
        public List<object> ChargeCoor { get; set; } = [];

        [JsonPropertyName("type")]
        public string Type = string.Empty;

        [JsonPropertyName("height")]
        public int Height = 0;

        [JsonPropertyName("width")]
        public int Width = 0;

        [JsonPropertyName("xAttrMin")]
        public int XAttrMin = 0;

        [JsonPropertyName("yAttrMin")]
        public int YAttrMin = 0;

        [JsonPropertyName("bgEle")]
        public List<BgElement> BgEle { get; set; } = [];
    }

    /// <summary>
    /// JSON structure for "gbEle" (background element) object from RCS compress.json.
    /// </summary>
    public class BgElement
    {
        [JsonPropertyName("x")]
        public int X { get; set; } = 0;

        [JsonPropertyName("y")]
        public int Y { get; set; } = 0;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; } = 0;

        [JsonPropertyName("ind")]
        public int Ind { get; set; } = 0;
    }

    /// <summary>
    /// RCS API shelf request structure.
    /// </summary>
    public class RequestShelf
    {
        [JsonPropertyName("areaId")]
        public string AreaId { get; set; } = "1";

        [JsonPropertyName("shelfNums")]
        public string ShelfNums { get; set; } = string.Empty;
    }

    /// <summary>
    /// RCS API shelf response structure.
    /// </summary>
    public class ResponseShelf
    {
        [JsonPropertyName("code")]
        public int Code { get; set; } = 0;

        [JsonPropertyName("data")]
        public List<ShelfInfo> Data { get; set; } = new();

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        public class ShelfInfo
        {
            [JsonPropertyName("angle")]
            public string Angle { get; set; } = string.Empty;

            [JsonPropertyName("posX")]
            public int PosX { get; set; } = 0;

            [JsonPropertyName("posY")]
            public int PosY { get; set; } = 0;

            [JsonPropertyName("shelfCurrStation")]
            public string ShelfCurrStation { get; set; } = string.Empty;

            [JsonPropertyName("shelfNum")]
            public string ShelfNum { get; set; } = string.Empty;

            [JsonPropertyName("shelfType")]
            public int ShelfType { get; set; } = 0;
        }
    }

    public class NodeList
    {
        public List<NodeData> List { get; set; } = new();
    }

    public class NodeData
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Type { get; set; } = 0;
        public string Content { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Isturn { get; set; } = false;
        public bool ShelfIsTurn { get; set; } = false;
        public List<object> ExtraTypes { get; set; } = [];
        public ShelfData ShelfData { get; set; } = new();
        public HttpClass.ContainerData ContainerData { get; set; } = new();
        public string ShowType { get; set; } = string.Empty;
    }

    public class ShelfList
    {
        public List<ShelfData> List { get; set; } = new();
    }

    public class ShelfData
    {
        public string Angle { get; set; } = string.Empty;
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public string ShelfCurrStation { get; set; } = string.Empty;
        public string ShelfNum { get; set; } = string.Empty;
        public int ShelfType { get; set; } = 0;
    }
}
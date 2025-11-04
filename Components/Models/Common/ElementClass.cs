namespace TaskManagerWeb.Components.Models.Common;
public static class Element
{
    public static class AMRState
    {
        public const string Busy = "Busy";
        public const string Charging = "Charging";
        public const string Disable = "Disable";
        public const string Faulty = "Faulty";
        public const string Idle = "Idle";
        public const string Init = "Init";
        public const string InTask = "In Task";
        public const string Moving = "Moving";
        public const string Offline = "Offline";
        public const string Online = "Online";
        public const string Unknown = "Unknown";
        public const string Upgrading = "Upgrading";
    }

    public static class TableState
    {
        public const string Empty = "Empty";
        public const string Full = "Full";
        public const string LoadOut = "LoadOut";
        public const string Loaded = "Loaded";
        public const string Quarantine = "Quarantine";

        public static readonly string[] All = [
            Full,
            Empty,
            Loaded,
            Quarantine
        ];
    }

    public static class TableType
    {
        public const string NA = "NA";
        public const string FG = "FG";
        public const string Gaylord = "Gaylord";
        public const string SKU = "SKU";

        public static readonly string[] All = [
            NA,
            FG,
            Gaylord,
            SKU
        ];
    }

    public class AMR
    {
        public int ID { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Status { get; set; } = AMRState.Unknown;
        public Position Position { get; set; } = new();
        public Battery Battery { get; set; } = new();
    }

    public class Position
    {
        public string Location { get; set; } = string.Empty;
        public double PosX { get; set; } = 0.0;
        public double PosY { get; set; } = 0.0;
    }

    public class Battery
    {
        public int Level { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
    }

    public class Table
    {
        public int ID { get; set; } = 0;
        public string TableCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LocCondition { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string KitNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
    }
}
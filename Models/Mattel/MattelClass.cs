using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TaskManagerWeb.Components.Models.Mattel;
public class MattelClass
{
    public class LayoutArea
    {
        public int ID { get; set; } = 0;
        public string LayoutName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LineCode { get; set; } = string.Empty;
        public string LayoutType { get; set; } = string.Empty;
        public int AreaCodeStart { get; set; } = 0;
        public int AreaCodeEnd { get; set; } = 0;
    }

    public class LayoutAreaList
    {
        public List<LayoutArea> LayoutArea { get; set; } = new();
    }

    public class LayoutTypeData
    {
        public int ID { get; set; } = 0;
        public string LayoutTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LayoutType { get; set; } = string.Empty;
        public int LocationQuantity { get; set; } = 0;
    }

    public class LayoutTypeList
    {
        public List<LayoutTypeData> LayoutTypeData { get; set; } = new();
    }

    public class ContainerData
    {
        public int ID { get; set; } = 0;
        public string ContainerType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BoxColor { get; set; } = string.Empty;
        public string FontColor { get; set; } = string.Empty;
    }

    public class ContainerTypeList
    {
        public List<ContainerData> List { get; set; } = new();
    }

    public class LineData
    {
        public int ID { get; set; } = 0;
        public string Point { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string BoxColor { get; set; } = string.Empty;
        public string FontColor { get; set; } = string.Empty;
        public bool CanDrop { get; set; } = true;
        public string TableType { get; set; } = string.Empty;
        public string TableBoxColor { get; set; } = string.Empty;
        public string TableFontColor { get; set; } = string.Empty;
        public bool TableCanDrop { get; set; } = true;

        [JsonIgnore]
        [NotMapped]
        public string GotTableColor { get; set; } = string.Empty;

        [JsonIgnore]
        [NotMapped]
        public string GotCommandColor { get; set; } = string.Empty;
    }

    public class LayoutData
    {
        public int ID { get; set; } = 0;
        public string LayoutName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LayoutTypeName { get; set; } = string.Empty;
        public List<LineData> LineData { get; set; } = new();

        [JsonIgnore]
        [NotMapped]
        public int countSKU { get; set; } = 0;

        [JsonIgnore]
        [NotMapped]
        public int countGL { get; set; } = 0;

        [JsonIgnore]
        [NotMapped]
        public int countFG { get; set; } = 0;
    }

    public class LayoutList
    {
        public List<LayoutData> List { get; set; } = new();
    }

    public class SheetData
    {
        public int ID { get; set; } = 0;
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Sheet
    {
        public int ID { get; set; } = 0;
        public string KitNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SheetData> SheetData { get; set; } = new();
    }

    public class SheetList
    {
        public List<Sheet> Sheet { get; set; } = new();
        public bool IsStillUse { get; set; } = false;
    }

    public class OrderKitData
    {
        public int ID { get; set; } = 0;
        public string Point { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CanDrop { get; set; } = false;
    }

    public class OrderKit
    {
        public int ID { get; set; } = 0;
        public string OrderKitNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LayoutName { get; set; } = string.Empty;
        public string KitNumber { get; set; } = string.Empty;
        public DateTime CreatedData { get; set; } = new();
        public List<OrderKitData> OrderKitData { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class OrderKitList
    {
        public List<OrderKit> OrderKit { get; set; } = new();
    }

    public class ProductionData
    {
        public int ID { get; set; } = 0;
        public string LineName { get; set; } = string.Empty;
        public string LineNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OrderKitNumber { get; set; } = string.Empty;
        public DateTime CreatedData { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class ProductionList
    {
        public List<ProductionData> ProductionData { get; set; } = new();
    }

    public class CheckInData
    {
        public int ID { get; set; } = 0;
        public string Point { get; set; } = string.Empty;
        public string KitNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CheckInList
    {
        public List<CheckInData> List { get; set; } = new();
    }

    public class MessageData
    {
        public int ID { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public string Text_1 { get; set; } = string.Empty;
        public string Text_2 { get; set; } = string.Empty;
        public string Text_3 { get; set; } = string.Empty;
        public string Text_4 { get; set; } = string.Empty;
    }

    public class MessageList
    {
        public List<MessageData> List { get; set; } = new();
    }

    public class SubscribeIdList
    {
        public List<string> SubscribeId { get; set; } = [];
    }

    public class RawData
    {
        public int ID { get; set; }
        public string? Raw { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Container
    {
        public int ID { get; set; } = 0;
        public string Location { get; set; } = string.Empty;
        public string ContainerId { get; set; } = string.Empty;
        public string LoadType { get; set; } = string.Empty;
        public string KitId { get; set; } = string.Empty;
        public string SkuId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string IndexDateTime { get; set; } = string.Empty;
        public string LastMove { get; set; } = string.Empty;
        public string LastLocation { get; set; } = string.Empty;
        public bool IsLock { get; set; } = false;
    }

    public class Page
    {
        public int ID { get; set; } = 0;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PageList
    {
        public List<Page> PageTypeList { get; set; } = [];
    }

    public class CPCSettings
    {
        public List<LocationSetting> LocationSettings { get; set; } = [];
    }
    public class LocationSetting
    {
        public string LocationID { get; set; } = string.Empty;
        public string Parameter { get; set; } = string.Empty;
    }

    public class CPCSetup
    {
        public string Header1 = string.Empty;
        public string Header2 = string.Empty;
        public bool IsLineEdit = false;
    }
}
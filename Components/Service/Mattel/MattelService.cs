using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Models.RCS;
using TaskManagerWeb.Components.Pages.Mattel;

namespace TaskManagerWeb.Components.Service.Mattel;
public class MattelService
{
    private static string taskName = "MattelService";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_MattelService;
    private static CommonLib commonLib = new();
    private string fileName1 = "wwwroot/json/LayoutAreaList.json";
    private string fileName2 = "wwwroot/json/LayoutTypeList.json";
    private string fileName3 = "wwwroot/json/ContainerTypeList.json";
    private string fileName4 = "wwwroot/json/LayoutList.json";
    private string fileName5 = "wwwroot/json/SheetList.json";
    private string fileName6 = "wwwroot/json/OrderKitList.json";
    private string fileName7 = "wwwroot/json/ProductionList.json";
    private string fileName8 = "wwwroot/json/ScanSheetList.json";
    private string fileName9 = "wwwroot/json/PageList.json";
    private string fileName10 = "wwwroot/json/CPCSettings.json";
    private bool isEncrypte = false;
    private MattelClass.LayoutAreaList layoutAreaList = new();
    private MattelClass.LayoutTypeList layoutTypeList = new();
    private MattelClass.ContainerTypeList containerTypeList = new();
    private static MattelClass.LayoutList layoutList = new();
    private MattelClass.SheetList sheetList = new();
    private MattelClass.SheetList scanSheetList = new();
    private MattelClass.SheetData backupPart = new();
    private static MattelClass.OrderKitList orderKitList = new();
    private MattelClass.ProductionList productionList = new();
    private MattelClass.CheckInList checkInList = new();
    private MattelClass.CheckInList actualInList = new();
    private MattelClass.ContainerTypeList containerInList = new();
    private MattelClass.MessageList msgListCheckIn = new();
    private MattelClass.PageList pageList = new();
    private RCSModel.NodeList nodeList = new();
    private RCSModel.ShelfList shelfList = new();
    private MattelClass.CPCSettings cPCSettings = new();

    public MattelService(CommonLib CommonLib)
    {
        commonLib = CommonLib;

        try
        {
            layoutAreaList.LayoutArea = new()
            {
                new (){
                    ID = 1,
                    LayoutName = "Line1",
                    Description = "Line #1",
                    LineCode = "L1",
                    LayoutType = "Production",
                    AreaCodeStart = 2206,
                    AreaCodeEnd = 2206
                },
                new (){
                    ID = 2,
                    LayoutName = "Line2",
                    Description = "Line #2",
                    LineCode = "L2",
                    LayoutType = "Production",
                    AreaCodeStart = 2205,
                    AreaCodeEnd = 2205
                },
                new (){
                    ID = 3,
                    LayoutName = "Line3",
                    Description = "Line #3",
                    LineCode = "L3",
                    LayoutType = "Production",
                    AreaCodeStart = 2204,
                    AreaCodeEnd = 2204
                },
                new (){
                    ID = 4,
                    LayoutName = "Line4",
                    Description = "Line #4",
                    LineCode = "L4",
                    LayoutType = "Production",
                    AreaCodeStart = 2203,
                    AreaCodeEnd = 2203
                },
                new (){
                    ID = 5,
                    LayoutName = "Line5",
                    Description = "Line #5",
                    LineCode = "L5",
                    LayoutType = "Production",
                    AreaCodeStart = 2202,
                    AreaCodeEnd = 2202
                },
                new (){
                    ID = 6,
                    LayoutName = "Line6",
                    Description = "Line #6",
                    LineCode = "L6",
                    LayoutType = "Production",
                    AreaCodeStart = 2201,
                    AreaCodeEnd = 2201
                },
                new (){
                    ID = 7,
                    LayoutName = "CheckIn",
                    Description = "Check In",
                    LineCode = "CI",
                    LayoutType = "CheckIn",
                    AreaCodeStart = 2001,
                    AreaCodeEnd = 2100
                },
                new (){
                    ID = 8,
                    LayoutName = "Gaylord",
                    Description = "Gaylord",
                    LineCode = "GL",
                    LayoutType = "Gaylord",
                    AreaCodeStart = 2401,
                    AreaCodeEnd = 2500
                },
                new (){
                    ID = 9,
                    LayoutName = "CPC",
                    Description = "CPC Area",
                    LineCode = "CPC",
                    LayoutType = "CPC",
                    AreaCodeStart = 2300,
                    AreaCodeEnd = 2400
                },
                new (){
                    ID = 10,
                    LayoutName = "VAS",
                    Description = "Staging",
                    LineCode = "VAS",
                    LayoutType = "VAS",
                    AreaCodeStart = 2101,
                    AreaCodeEnd = 2200
                },
                new (){
                    ID = 11,
                    LayoutName = "AMR",
                    Description = "AMR maintenance",
                    LineCode = "AMR",
                    LayoutType = "AMR",
                    AreaCodeStart = 9999,
                    AreaCodeEnd = 9999
                },
                new (){
                    ID = 12,
                    LayoutName = "Table",
                    Description = "Table maintenance",
                    LineCode = "Table",
                    LayoutType = "Table",
                    AreaCodeStart = 9999,
                    AreaCodeEnd = 9999
                }
            };

            commonLib.JsonSerializeFile(
                layoutAreaList,
                fileName1,
                IsEncrypted: isEncrypte
            );
            layoutAreaList = commonLib.JsonDeserializeFile<MattelClass.LayoutAreaList>(fileName1, isEncrypte)
                ?? new();

            layoutTypeList.LayoutTypeData = new()
            {
                new (){
                    ID = 1,
                    LayoutTypeName = "Standard",
                    LayoutType = "Production",
                    Description = "Single line map",
                    LocationQuantity = 30
                },
                new (){
                    ID = 2,
                    LayoutTypeName = "Island",
                    LayoutType = "Production",
                    Description = "Island map",
                    LocationQuantity = 32
                },
                new (){
                    ID = 3,
                    LayoutTypeName = "CheckIn",
                    LayoutType = "CheckIn",
                    Description = "Check in map",
                    LocationQuantity = 7
                },
                new (){
                    ID = 4,
                    LayoutTypeName = "Gaylord",
                    LayoutType = "Gaylord",
                    Description = "Dumpster area",
                    LocationQuantity = 6
                },
                new (){
                    ID = 5,
                    LayoutTypeName = "CPC",
                    LayoutType = "CPC",
                    Description = "Check out area",
                    LocationQuantity = 12
                },
                new (){
                    ID = 5,
                    LayoutTypeName = "VAS",
                    LayoutType = "VAS",
                    Description = "Staging area",
                    LocationQuantity = 20
                }
            };

            commonLib.JsonSerializeFile(
                layoutTypeList,
                fileName2,
                IsEncrypted: isEncrypte
            );
            layoutTypeList = commonLib.JsonDeserializeFile<MattelClass.LayoutTypeList>(fileName2, isEncrypte)
                ?? new();

            containerTypeList.List = new()
            {
                new (){
                    ID = 1,
                    ContainerType = "NA",
                    Description = "Not available",
                    BoxColor = "#d3d3d3",
                    FontColor = "black"
                },
                new (){
                    ID = 2,
                    ContainerType = "TBL",
                    Description = "Table",
                    BoxColor = "#a2a2a2",
                    FontColor = "white"
                },
                new (){
                    ID = 3,
                    ContainerType = "SKU",
                    Description = "Material",
                    BoxColor = "#ffbd62",
                    FontColor = "black"
                },
                new (){
                    ID = 4,
                    ContainerType = "GL",
                    Description = "Gaylord",
                    BoxColor = "#874f41",
                    FontColor = "white"
                },
                new (){
                    ID = 5,
                    ContainerType = "FG",
                    Description = "Finished goods",
                    BoxColor = "#f36d33",
                    FontColor = "white"
                }
            };

            commonLib.JsonSerializeFile(
                containerTypeList,
                fileName3,
                IsEncrypted: isEncrypte
            );
            containerTypeList = commonLib.JsonDeserializeFile<MattelClass.ContainerTypeList>(fileName3, isEncrypte)
                ?? new();

            // layoutList.List = new()
            // {
            //     new(){
            //         ID = 1,
            //         LayoutName = "L001",
            //         Description = string.Empty,
            //         LayoutTypeName = "Standard",
            //         LineData = new(){
            //             new(){
            //                 ID = 1,
            //                 Point = "0001",
            //                 Description = string.Empty,
            //                 ContainerType = "SKU",
            //                 BoxColor = "#ffbd62",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 2,
            //                 Point = "0002",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 3,
            //                 Point = "0003",
            //                 Description = string.Empty,
            //                 ContainerType = "SKU",
            //                 BoxColor = "#ffbd62",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 4,
            //                 Point = "0004",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 5,
            //                 Point = "0005",
            //                 Description = string.Empty,
            //                 ContainerType = "GL",
            //                 BoxColor = "#874f41",
            //                 FontColor = "white",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 6,
            //                 Point = "0006",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 7,
            //                 Point = "0007",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 8,
            //                 Point = "0008",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 9,
            //                 Point = "0009",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 10,
            //                 Point = "0010",
            //                 Description = string.Empty,
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 ContainerType = "NA"
            //             },
            //             new(){
            //                 ID = 11,
            //                 Point = "0011",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 12,
            //                 Point = "0012",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 13,
            //                 Point = "0013",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 14,
            //                 Point = "0014",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 15,
            //                 Point = "0015",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 16,
            //                 Point = "0016",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 17,
            //                 Point = "0017",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 18,
            //                 Point = "0018",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 19,
            //                 Point = "0019",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 20,
            //                 Point = "0020",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 21,
            //                 Point = "0021",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 22,
            //                 Point = "0022",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 23,
            //                 Point = "0023",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 24,
            //                 Point = "0024",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 25,
            //                 Point = "0025",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 26,
            //                 Point = "0026",
            //                 Description = string.Empty,
            //                 ContainerType = "NA",
            //                 BoxColor = "#d3d3d3",
            //                 FontColor = "black",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 27,
            //                 Point = "0027",
            //                 Description = string.Empty,
            //                 ContainerType = "FG",
            //                 BoxColor = "#f36d33",
            //                 FontColor = "white",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 28,
            //                 Point = "0028",
            //                 Description = string.Empty,
            //                 ContainerType = "FG",
            //                 BoxColor = "#f36d33",
            //                 FontColor = "white",
            //                 CanDrop = true
            //             }
            //         }
            //     }
            // };


            commonLib.JsonSerializeFile(
                layoutList,
                fileName4,
                IsEncrypted: isEncrypte
            );
            layoutList = commonLib.JsonDeserializeFile<MattelClass.LayoutList>(fileName4, isEncrypte)
                ?? new();

            foreach (var item in layoutList.List)
            {
                ProcessLayoutList(item);
            }

            // sheetList.Sheet = new()
            // {
            //     new()
            //     {
            //         ID = 1,
            //         KitNumber = "P-432154",
            //         Description = string.Empty,
            //         SheetData = new()
            //         {
            //             new(){
            //                 ID = 1,
            //                 PartNumber = "DVF50-958J",
            //                 Description = "BRBMBRLTYC"
            //             },
            //             new(){
            //                 ID = 2,
            //                 PartNumber = "FBR37-968J",
            //                 Description = "BRBFSHNSTA"
            //             },
            //             new(){
            //                 ID = 3,
            //                 PartNumber = "HRH11-9564",
            //                 Description = "BRBFASHDLS"
            //             },
            //             new(){
            //                 ID = 4,
            //                 PartNumber = "HRH12-9564",
            //                 Description = "BRBFASHDIS"
            //             },
            //             new(){
            //                 ID = 5,
            //                 PartNumber = "HRH15-9564",
            //                 Description = "BRBFASHDLS"
            //             },
            //             new(){
            //                 ID = 6,
            //                 PartNumber = "HRH22-9564",
            //                 Description = "BRBFASHDLF"
            //             },
            //             new(){
            //                 ID = 7,
            //                 PartNumber = "HTH66-9796",
            //                 Description = "BRBGCANNVD"
            //             }
            //         }
            //     },
            //     new()
            //     {
            //         ID = 2,
            //         KitNumber = "P-132154",
            //         Description = string.Empty,
            //         SheetData = new()
            //         {
            //             new(){
            //                 ID = 1,
            //                 PartNumber = "DDF50-958J",
            //                 Description = "BRBMBRLTYC"
            //             },
            //             new(){
            //                 ID = 2,
            //                 PartNumber = "GBR37-968J",
            //                 Description = "BRBFSHNSTA"
            //             },
            //             new(){
            //                 ID = 3,
            //                 PartNumber = "SSH11-9564",
            //                 Description = "BRBFASHDLS"
            //             },
            //             new(){
            //                 ID = 4,
            //                 PartNumber = "SSH12-9564",
            //                 Description = "BRBFASHDIS"
            //             },
            //             new(){
            //                 ID = 5,
            //                 PartNumber = "HRH15-9564",
            //                 Description = "BRBFASHDLS"
            //             },
            //             new(){
            //                 ID = 6,
            //                 PartNumber = "HRH22-9564",
            //                 Description = "BRBFASHDLF"
            //             },
            //             new(){
            //                 ID = 7,
            //                 PartNumber = "HTH66-9796",
            //                 Description = "BRBGCANNVD"
            //             }
            //         }
            //     }
            // };

            commonLib.JsonSerializeFile(
                sheetList,
                fileName5,
                IsEncrypted: isEncrypte
            );
            sheetList = commonLib.JsonDeserializeFile<MattelClass.SheetList>(fileName5, isEncrypte)
                ?? new();

            // orderKitList.OrderKit = new()
            // {
            //     new(){
            //         ID = 1,
            //         OrderKitNumber = "P-432154",
            //         Description = string.Empty,
            //         LayoutName = "L001",
            //         KitNumber = "P-432154",
            //         CreatedData = DateTime.Now,
            //         Status = string.Empty,
            //         OrderKitData = new(){
            //             new(){
            //                 ID = 1,
            //                 Point = "0001",
            //                 PartNumber = "DVF50-958J",
            //                 Description = "BRBMBRLTYC",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 2,
            //                 Point = "0002",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 3,
            //                 Point = "0003",
            //                 PartNumber = "FBR37-968J",
            //                 Description = "BRBFSHNSTA",
            //                 CanDrop = true
            //             },
            //             new(){
            //                 ID = 4,
            //                 Point = "0004",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 5,
            //                 Point = "0005",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 6,
            //                 Point = "0006",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 7,
            //                 Point = "0007",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 8,
            //                 Point = "0008",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 9,
            //                 Point = "0009",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 10,
            //                 Point = "0010",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 11,
            //                 Point = "0011",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 12,
            //                 Point = "0012",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 13,
            //                 Point = "0013",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 14,
            //                 Point = "0014",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 15,
            //                 Point = "0015",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 16,
            //                 Point = "0016",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 17,
            //                 Point = "0017",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 18,
            //                 Point = "0018",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 19,
            //                 Point = "0019",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 20,
            //                 Point = "0020",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 21,
            //                 Point = "0021",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 22,
            //                 Point = "0022",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 23,
            //                 Point = "0023",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 24,
            //                 Point = "0024",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 25,
            //                 Point = "0025",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 26,
            //                 Point = "0026",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 27,
            //                 Point = "0027",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             },
            //             new(){
            //                 ID = 28,
            //                 Point = "0028",
            //                 PartNumber = string.Empty,
            //                 Description = string.Empty,
            //                 CanDrop = false
            //             }
            //         }
            //     }
            // };

            commonLib.JsonSerializeFile(
                orderKitList,
                fileName6,
                IsEncrypted: isEncrypte
            );
            orderKitList = commonLib.JsonDeserializeFile<MattelClass.OrderKitList>(fileName6, isEncrypte)
                ?? new();

            productionList.ProductionData = new()
            {
                new(){
                    ID = 1,
                    LineName = "Line 1",
                    LineNumber = "2201",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                },
                new(){
                    ID = 2,
                    LineName = "Line 2",
                    LineNumber = "2202",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                },
                new(){
                    ID = 3,
                    LineName = "Line 3",
                    LineNumber = "2203",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                },
                new(){
                    ID = 4,
                    LineName = "Line 4",
                    LineNumber = "2204",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                },
                new(){
                    ID = 5,
                    LineName = "Line 5",
                    LineNumber = "2205",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                },
                new(){
                    ID = 6,
                    LineName = "Line 6",
                    LineNumber = "2206",
                    Description = string.Empty,
                    OrderKitNumber = string.Empty,
                    CreatedData = DateTime.Now,
                    Status = string.Empty
                }
            };

            commonLib.JsonSerializeFile(
                productionList,
                fileName7,
                IsEncrypted: isEncrypte
            );
            productionList = commonLib.JsonDeserializeFile<MattelClass.ProductionList>(fileName7, isEncrypte)
                ?? new();

            actualInList.List = new()
            {
                new(){
                    ID = 1,
                    Point = "0001",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 2,
                    Point = "0002",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 3,
                    Point = "0003",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 4,
                    Point = "0004",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 5,
                    Point = "0005",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 6,
                    Point = "0006",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                },
                new(){
                    ID = 7,
                    Point = "0007",
                    KitNumber = string.Empty,
                    PartNumber = string.Empty
                }
            };

            containerInList.List = new()
            {
                new(){
                    ID = 1,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 2,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 3,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 4,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 5,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 6,
                    ContainerType = "Empty",
                },
                new(){
                    ID = 7,
                    ContainerType = "Empty",
                }
            };

            // scanSheetList.Sheet = new()
            // {
            // new()
            // {
            //     ID = 1,
            //     KitNumber = "P-432154",
            //     Description = string.Empty,
            //     SheetData = new()
            //     {
            //         new(){
            //             ID = 1,
            //             PartNumber = "DVF50-958J",
            //             Description = "BRBMBRLTYC"
            //         },
            //         new(){
            //             ID = 2,
            //             PartNumber = "FBR37-968J",
            //             Description = "BRBFSHNSTA"
            //         },
            //         new(){
            //             ID = 3,
            //             PartNumber = "HRH11-9564",
            //             Description = "BRBFASHDLS"
            //         },
            //         new(){
            //             ID = 4,
            //             PartNumber = "HRH12-9564",
            //             Description = "BRBFASHDIS"
            //         },
            //         new(){
            //             ID = 5,
            //             PartNumber = "HRH15-9564",
            //             Description = "BRBFASHDLS"
            //         },
            //         new(){
            //             ID = 6,
            //             PartNumber = "HRH22-9564",
            //             Description = "BRBFASHDLF"
            //         },
            //         new(){
            //             ID = 7,
            //             PartNumber = "HTH66-9796",
            //             Description = "BRBGCANNVD"
            //         }
            //     }
            // }
            // };

            commonLib.JsonSerializeFile(
                scanSheetList,
                fileName8,
                IsEncrypted: isEncrypte
            );
            scanSheetList = commonLib.JsonDeserializeFile<MattelClass.SheetList>(fileName8, isEncrypte)
                ?? new();

            if (scanSheetList.IsStillUse)
            {
                scanSheetList.IsStillUse = false;
                commonLib.JsonSerializeFile(
                    scanSheetList,
                    fileName8,
                    true,
                    isEncrypte
                );
            }

            pageList.PageTypeList = new()
            {
                new (){
                    ID = 1,
                    Type = "Disable",
                    Description = "Disable"
                },
                new (){
                    ID = 2,
                    Type = "L1",
                    Description = "Line 1"
                },
                new (){
                    ID = 3,
                    Type = "L2",
                    Description = "Line 2"
                },
                new (){
                    ID = 4,
                    Type = "L3",
                    Description = "Line 3"
                },
                new (){
                    ID = 5,
                    Type = "L4",
                    Description = "Line 4"
                },
                new (){
                    ID = 6,
                    Type = "L5",
                    Description = "Line 5"
                },
                new (){
                    ID = 7,
                    Type = "L6",
                    Description = "Line 6"
                },
                new (){
                    ID = 8,
                    Type = "CI",
                    Description = "Check In"
                },
                new (){
                    ID = 9,
                    Type = "CO",
                    Description = "Check Out"
                },
                new (){
                    ID = 10,
                    Type = "CPC",
                    Description = "Checkpoint Charlie"
                },
                new (){
                    ID = 11,
                    Type = "GL",
                    Description = "Gaylord"
                }
            };

            commonLib.JsonSerializeFile(
                pageList,
                fileName9,
                IsEncrypted: isEncrypte
            );
            pageList = commonLib.JsonDeserializeFile<MattelClass.PageList>(fileName9, isEncrypte)
                ?? new();

            cPCSettings.LocationSettings = new()
            {
                new(){
                    LocationID = "23000001",
                    Parameter = "L1"
                },
                new(){
                    LocationID = "23000002",
                    Parameter = "L1"
                },
                new(){
                    LocationID = "23000004",
                    Parameter = "L2"
                },
                new(){
                    LocationID = "23000005",
                    Parameter = "L2"
                },
                new(){
                    LocationID = "23000006",
                    Parameter = "L3"
                },
                new(){
                    LocationID = "23000007",
                    Parameter = "L3"
                },
                new(){
                    LocationID = "23000008",
                    Parameter = "L4"
                },
                new(){
                    LocationID = "23000009",
                    Parameter = "L4"
                },
                new(){
                    LocationID = "23000010",
                    Parameter = "L5"
                },
                new(){
                    LocationID = "23000012",
                    Parameter = "L5"
                },
                new(){
                    LocationID = "23000013",
                    Parameter = "L6"
                },
                new(){
                    LocationID = "23000014",
                    Parameter = "L6"
                }
            };

            commonLib.JsonSerializeFile(
                cPCSettings,
                fileName10,
                IsEncrypted: isEncrypte
            );
            cPCSettings = commonLib.JsonDeserializeFile<MattelClass.CPCSettings>(fileName10, isEncrypte)
                ?? new();
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

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 1,
        //     Title = "L1 Reg | VAS",
        //     Text_1 = "Kit #  : P-405934",
        //     Text_2 = "Part # : DVF50-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 2,
        //     Title = "L2 Reg | VAS",
        //     Text_1 = "Kit #  : S-345934",
        //     Text_2 = "Part # : HRHW-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 3,
        //     Title = "L3 Reg | VAS",
        //     Text_1 = "Kit #  : P-405934",
        //     Text_2 = "Part # : DVF50-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 4,
        //     Title = "L4 Reg | VAS",
        //     Text_1 = "Kit #  : S-345934",
        //     Text_2 = "Part # : HRHW-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 5,
        //     Title = "L5 Reg | VAS",
        //     Text_1 = "Kit #  : P-405934",
        //     Text_2 = "Part # : DVF50-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 6,
        //     Title = "L6 Reg | VAS",
        //     Text_1 = "Kit #  : S-345934",
        //     Text_2 = "Part # : HRHW-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 7,
        //     Title = "L7 Reg | VAS",
        //     Text_1 = "Kit #  : S-345934",
        //     Text_2 = "Part # : HRHW-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });

        // msgListCheckIn.List.Add(new()
        // {
        //     ID = 8,
        //     Title = "L8 Reg | VAS",
        //     Text_1 = "Kit #  : S-345934",
        //     Text_2 = "Part # : HRHW-901J",
        //     Text_3 = "Status : Pickup",
        //     Text_4 = "Waiting AMR to pickup"
        // });
    }

    public LayoutMethod Layout { get; } = new();
    public class LayoutMethod
    {
        public bool IsLayoutUsed(string LayoutName)
        {
            commonLib.DisplayConsole(
                taskName,
                $"LayoutMethod: IsNotUse triggered",
                settingLevel,
                MessageLevel.Information
            );

            try
            {
                bool state = false;
                var findData = orderKitList.OrderKit.FirstOrDefault(
                    n => n.LayoutName == LayoutName
                );

                if (findData != null)
                    state = true;

                return state;
            }
            catch (Exception ex)
            {
                commonLib.DisplayConsole(
                    $"{taskName}: LayoutMethod: IsNotUse",
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }

            return true;
        }
    }

    public KitMethod Kit { get; } = new();
    public class KitMethod
    {
        public bool IsKitUsed(string KitNumber)
        {
            commonLib.DisplayConsole(
                taskName,
                $"KitMethod: IsNotUse triggered",
                settingLevel,
                MessageLevel.Information
            );

            try
            {
                bool state = false;
                var findData = orderKitList.OrderKit.FirstOrDefault(
                    n => n.KitNumber == KitNumber
                );

                if (findData != null)
                    state = true;

                return state;
            }
            catch (Exception ex)
            {
                commonLib.DisplayConsole(
                    $"{taskName}: KitMethod: IsNotUse",
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }

            return true;
        }
    }

    public MattelClass.LayoutAreaList GetLayoutAreaList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetLayoutAreaList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return layoutAreaList.DeepClone();
    }

    public MattelClass.LayoutTypeList GetLayoutTypeList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetLayoutTypeList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return layoutTypeList.DeepClone();
    }

    public MattelClass.ContainerTypeList GetContainerTypeList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetContainerTypeList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return containerTypeList.DeepClone();
    }

    public void SetContainerTypeList(MattelClass.ContainerTypeList ContainerTypeList)
    {
        containerTypeList = ContainerTypeList;

        commonLib!.JsonSerializeFile(
            containerTypeList,
            fileName3,
            true,
            isEncrypte
        );
    }

    public MattelClass.LayoutList GetLayoutList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetLayoutList triggered",
            settingLevel,
            MessageLevel.Information
        );

        var tempData = layoutList.DeepClone();

        foreach (var item in tempData.List)
        {
            ProcessLayoutList(item);
        }

        return tempData;
    }

    public bool IsDuplicateLayoutName(MattelClass.LayoutData LayoutData)
    {
        bool result = false;

        var findData = layoutList.List.FirstOrDefault(
            n => n.LayoutName == LayoutData.LayoutName
        );

        if (findData != null)
            result = true;

        return result;
    }

    public void AddLayoutData(MattelClass.LayoutData LayoutData)
    {
        ProcessLayoutList(LayoutData);
        LayoutData.ID =
            layoutList.List.Count != 0
            ? layoutList.List.Max(p => p.ID) + 1
            : 1;
        layoutList.List.Add(LayoutData);

        commonLib!.JsonSerializeFile(
            layoutList,
            fileName4,
            true,
            isEncrypte
        );
    }

    public void EditLayoutData(MattelClass.LayoutData LayoutData)
    {
        var findData = layoutList.List.FirstOrDefault(
            n => n.ID == LayoutData.ID
        );

        if (findData != null)
        {
            findData.LayoutTypeName = LayoutData.LayoutTypeName;
            findData.LineData = LayoutData.LineData;
            findData.countFG = LayoutData.countFG;
            findData.countGL = LayoutData.countGL;
            findData.countSKU = LayoutData.countSKU;
            ProcessLayoutList(findData);

            commonLib!.JsonSerializeFile(
                layoutList,
                fileName4,
                true,
                isEncrypte
            );
        }
    }

    public void ProcessLayoutList(MattelClass.LayoutData LayoutData)
    {
        if (LayoutData != null)
        {
            LayoutData.countSKU = LayoutData.LineData.Count(n => n.ContainerType == "SKU");
            LayoutData.countGL = LayoutData.LineData.Count(n => n.ContainerType == "GL");
            LayoutData.countFG = LayoutData.LineData.Count(n => n.ContainerType == "FG");
        }
    }

    public void RemoveLayoutData(MattelClass.LayoutData LayoutData)
    {
        var findData = layoutList.List.FirstOrDefault(
            n => n.ID == LayoutData.ID
        );

        if (findData != null)
            layoutList.List.Remove(findData);

        commonLib!.JsonSerializeFile(
            layoutList,
            fileName4,
            true,
            isEncrypte
        );
    }

    public MattelClass.SheetList GetSheetList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetSheetList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return sheetList.DeepClone();
    }

    public void SetSheetList(MattelClass.SheetList SheetList)
    {
        sheetList = SheetList;

        commonLib!.JsonSerializeFile(
            sheetList,
            fileName5,
            true,
            isEncrypte
        );
    }

    public bool CheckKitNumber(string kitNumber)
    {
        return sheetList.Sheet.Any(s => s.KitNumber == kitNumber);
    }

    public bool CheckPartNumber(string partNumber)
    {
        return sheetList.Sheet.Any(s => s.SheetData.Any(p => p.PartNumber == partNumber));
    }

    public void AddKit(MattelClass.Sheet newSheet)
    {
        if (!CheckKitNumber(newSheet.KitNumber))
        {
            newSheet.ID =
                sheetList.Sheet.Count != 0
                ? sheetList.Sheet.Max(s => s.ID) + 1
                : 1;
            sheetList.Sheet.Add(newSheet);
            SetSheetList(sheetList);
        }
    }

    public void AddPart(string kitNumber, MattelClass.SheetData newPart)
    {
        var sheet = sheetList.Sheet.FirstOrDefault(s => s.KitNumber == kitNumber);
        if (sheet != null && !CheckPartNumber(newPart.PartNumber))
        {
            newPart.ID =
                sheet.SheetData.Count != 0
                ? sheet.SheetData.Max(p => p.ID) + 1
                : 1;
            sheet.SheetData.Add(newPart);
            SetSheetList(sheetList);
        }
    }

    public void EditKit(MattelClass.Sheet updatedSheet)
    {
        var sheet = sheetList.Sheet.FirstOrDefault(s => s.ID == updatedSheet.ID);
        if (sheet != null)
        {
            sheet.KitNumber = updatedSheet.KitNumber;
            sheet.Description = updatedSheet.Description;
            sheet.SheetData = updatedSheet.SheetData;
            SetSheetList(sheetList);
        }
    }

    public void EditPart(string kitNumber, MattelClass.SheetData updatedPart)
    {
        var sheet = sheetList.Sheet.FirstOrDefault(s => s.KitNumber == kitNumber);
        if (sheet != null)
        {
            var part = sheet.SheetData.FirstOrDefault(p => p.ID == updatedPart.ID);
            if (part != null)
            {
                part.PartNumber = updatedPart.PartNumber;
                part.Description = updatedPart.Description;
                SetSheetList(sheetList);
            }
        }
    }

    public void RemoveKit(MattelClass.Sheet removeSheet)
    {
        var findData = sheetList.Sheet.FirstOrDefault(
            n => n.ID == removeSheet.ID
        );

        if (findData != null)
        {
            sheetList.Sheet.Remove(findData);

            commonLib!.JsonSerializeFile(
                sheetList,
                fileName5,
                true,
                isEncrypte
            );
        }
    }

    public void BackupPart(MattelClass.SheetData part)
    {
        backupPart = part;
    }

    public MattelClass.SheetData GetBackupPart()
    {
        return backupPart;
    }
    public void ClearBackupPart()
    {
        backupPart = new MattelClass.SheetData();
    }

    public MattelClass.SheetList GetScanSheetList()
    {
        return scanSheetList.DeepClone();
    }

    public void SaveScanSheetList(MattelClass.SheetList SheetList)
    {
        scanSheetList = SheetList;

        commonLib!.JsonSerializeFile(
            sheetList,
            fileName8,
            true,
            isEncrypte
        );
    }

    public MattelClass.OrderKitList GetOrderKitList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetOrderKitList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return orderKitList.DeepClone();
    }

    public bool IsDuplicateOrderKitNumber(MattelClass.OrderKit OrderKit)
    {
        bool result = false;
        var findData = orderKitList.OrderKit.FirstOrDefault(
            n => n.KitNumber == OrderKit.KitNumber
            || n.OrderKitNumber == OrderKit.OrderKitNumber
        );

        if (findData != null)
            result = true;

        return result;
    }

    public bool IsDuplicateOrderKitNumber(string OrderKitNumber)
    {
        bool result = false;
        var findData = orderKitList.OrderKit.FirstOrDefault(
            n => n.KitNumber == OrderKitNumber
            || n.OrderKitNumber == OrderKitNumber
        );

        if (findData != null)
            result = true;

        return result;
    }

    public void AddOrderKit(MattelClass.OrderKit OrderKit)
    {
        OrderKit.ID =
            orderKitList.OrderKit.Count != 0
            ? orderKitList.OrderKit.Max(p => p.ID) + 1
            : 1;
        orderKitList.OrderKit.Add(OrderKit);

        commonLib!.JsonSerializeFile(
            orderKitList,
            fileName6,
            true,
            isEncrypte
        );
    }

    public void EditOrderKit(MattelClass.OrderKit OrderKit)
    {
        var findData = orderKitList.OrderKit.FirstOrDefault(
            n => n.ID == OrderKit.ID
        );

        if (findData != null)
        {
            findData.OrderKitNumber = OrderKit.OrderKitNumber;
            findData.Description = OrderKit.Description;
            findData.LayoutName = OrderKit.LayoutName;
            findData.KitNumber = OrderKit.KitNumber;
            findData.CreatedData = OrderKit.CreatedData;
            findData.OrderKitData = OrderKit.OrderKitData;

            commonLib!.JsonSerializeFile(
                orderKitList,
                fileName6,
                true,
                isEncrypte
            );
        }
    }

    public void StatusOrderKit(string OrderKitNumber, string Status)
    {
        var findData = orderKitList.OrderKit.FirstOrDefault(
            n => n.OrderKitNumber == OrderKitNumber
        );

        if (findData != null)
        {
            findData.Status = Status;

            commonLib!.JsonSerializeFile(
                orderKitList,
                fileName6,
                true,
                isEncrypte
            );
        }
    }

    public void RemoveOrderKit(MattelClass.OrderKit OrderKit)
    {
        var findData = orderKitList.OrderKit.FirstOrDefault(
            n => n.ID == OrderKit.ID
        );

        if (findData != null)
            orderKitList.OrderKit.Remove(findData);

        commonLib!.JsonSerializeFile(
            orderKitList,
            fileName6,
            true,
            isEncrypte
        );
    }

    public MattelClass.ProductionList GetProductionList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetProductionList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return productionList.DeepClone();
    }

    public void UpdateProductionList(MattelClass.ProductionData ProductionData)
    {
        var updateData = productionList.ProductionData.FirstOrDefault(
            n => n.ID == ProductionData.ID
        );

        if (updateData != null)
        {
            updateData.OrderKitNumber = ProductionData.OrderKitNumber;
            updateData.Status = ProductionData.Status;
            updateData.CreatedData = ProductionData.CreatedData;

            commonLib!.JsonSerializeFile(
                productionList,
                fileName7,
                true,
                isEncrypte
            );
        }
    }

    public MattelClass.CheckInList GetCheckInList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetCheckInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return checkInList;
    }

    public void SetCheckInList(MattelClass.CheckInList CheckInList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetCheckInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        checkInList = CheckInList;
    }

    public MattelClass.CheckInList GetActualInList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetActualInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return actualInList;
    }

    public void SetActualInList(MattelClass.CheckInList ActualInList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetActualInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        actualInList = ActualInList;
    }

    public MattelClass.ContainerTypeList GetContainerInList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetContainerInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return containerInList.DeepClone();
    }

    public void SetContainerInList(MattelClass.ContainerTypeList ContainerInList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetContainerInList triggered",
            settingLevel,
            MessageLevel.Information
        );

        containerInList = ContainerInList;
    }

    public MattelClass.MessageList GetMsgListCheckIn()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetMsgListCheckIn triggered",
            settingLevel,
            MessageLevel.Information
        );

        return msgListCheckIn;
    }

    public void SetMsgListCheckIn(MattelClass.MessageList MessageList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetMsgListCheckIn triggered",
            settingLevel,
            MessageLevel.Information
        );

        msgListCheckIn = MessageList;
    }

    public RCSModel.NodeList GetRCSNodeList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetRCSNodeList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return nodeList.DeepClone();
    }

    public void SetRCSNodeList(RCSModel.NodeList NodeList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetRCSNodeList triggered",
            settingLevel,
            MessageLevel.Information
        );

        nodeList = NodeList;
    }

    public MattelClass.PageList GetPageList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetPageList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return pageList.DeepClone();
    }

    public RCSModel.ShelfList GetRCSShelfList()
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetRCSShelfList triggered",
            settingLevel,
            MessageLevel.Information
        );

        return shelfList.DeepClone();
    }

    public void SetRCSShelfList(RCSModel.ShelfList ShelfList)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SetRCSShelfList triggered",
            settingLevel,
            MessageLevel.Information
        );

        shelfList = ShelfList;
    }

    public MattelClass.CPCSettings GetCPCSettings()
    {
        return cPCSettings.DeepClone();
    }

    public void SetCPCSettings(MattelClass.CPCSettings CPCSettings)
    {
        cPCSettings = CPCSettings;

        commonLib!.JsonSerializeFile(
            cPCSettings,
            fileName10,
            true,
            isEncrypte
        );
    }

    public void UpdateCPCLocations(List<MattelClass.LocationSetting> LocSettings)
    {
        if (LocSettings?.Count > 0)
        {
            foreach (var item in LocSettings)
            {
                var findData = cPCSettings.LocationSettings.FirstOrDefault(
                    n => n.LocationID == item.LocationID
                );

                if (findData != null)
                {
                    findData.Parameter = item.Parameter;
                }
            }

            commonLib!.JsonSerializeFile(
                cPCSettings,
                fileName10,
                true,
                isEncrypte
            );
        }
    }
}
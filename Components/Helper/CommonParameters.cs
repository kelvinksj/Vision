namespace TaskManagerWeb.Components.Helper;
public static class HttpParameters
{
    public static string Endpoint_postman = "https://639685e9-361e-417c-b241-e3c74d544f5d.mock.pstmn.io";

    public static string MyHeader { get; set; } = "http";
    public static string MyIP { get; set; } = "70.70.70.120";
    public static string MyPort { get; set; } = "20000";
    public static string MyEndpoint { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}";

    public static string ServiceHeader { get; set; } = "http";
    public static string ServiceIP { get; set; } = "70.70.70.124";
    public static string ServicePort { get; set; } = "20040";
    public static string ServiceEndpoint_uicommand { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uicommand";
    public static string ServiceEndpoint_uicancel { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uicancel";
    public static string ServiceEndpoint_uisubscribe { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uisubscribe";
    public static string ServiceEndpoint_uiamrsubscribe { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uiamrsubscribe";
    public static string ServiceEndpoint_uiamrcommand { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uiamrcommand";
    public static string ServiceEndpoint_uitablecommand { get; set; } =
        $"{ServiceHeader}://{ServiceIP}:{ServicePort}/uitablecommand";

    public static string ContainerHeader { get; set; } = "http";
    public static string ContainerIP { get; set; } = "70.70.70.125";
    public static string ContainerPort { get; set; } = "20060";
    public static string ContainerEndpoint_locationSubPub { get; set; } =
        $"{ContainerHeader}://{ContainerIP}:{ContainerPort}/locationSubPub";
    public static string ContainerEndpoint_cancelSubscription { get; set; } =
        $"{ContainerHeader}://{ContainerIP}:{ContainerPort}/cancelSubscription";

    public static string ContainerHeader2 { get; set; } = "http";
    public static string ContainerIP2 { get; set; } = "70.70.70.125";
    public static string ContainerPort2 { get; set; } = "20051";
    public static string ContainerEndpoint_uicommand { get; set; } =
        $"{ContainerHeader2}://{ContainerIP2}:{ContainerPort2}/uicommand";

    public static string ScanHeader { get; set; } = "http";
    public static string ScanIP { get; set; } = "70.70.70.126";
    public static string ScanPort { get; set; } = "20070";
    public static string ScanEndpoint_scan { get; set; } =
        $"{ScanHeader}://{ScanIP}:{ScanPort}/scan";
    public static string ScanEndpoint_scanprogress { get; set; } =
        $"{ScanHeader}://{ScanIP}:{ScanPort}/scanprogress";

    private static int callID { get; set; } = 1;
    public static int CallID
    {
        get
        {
            if (callID > 99999) callID = 1;
            return callID++;
        }

        set { callID = value; }
    }

    public static void SetMyIP(bool IsHttps, string IP, string Port)
    {
        string header = IsHttps ? "https" : "http";
        MyHeader = header;
        MyIP = IP;
        MyPort = Port;
        MyEndpoint = $"{header}://{IP}:{Port}";
    }

    public static void SetServiceIP(bool IsHttps, string IP, string Port)
    {
        string header = IsHttps ? "https" : "http";
        ServiceHeader = header;
        ServiceIP = IP;
        ServicePort = Port;
        ServiceEndpoint_uicommand = $"{header}://{IP}:{Port}/uicommand";
        ServiceEndpoint_uicancel = $"{header}://{IP}:{Port}/uicancel";
        ServiceEndpoint_uisubscribe = $"{header}://{IP}:{Port}/uisubscribe";
        ServiceEndpoint_uiamrsubscribe = $"{header}://{IP}:{Port}/uiamrsubscribe";
        ServiceEndpoint_uiamrcommand = $"{header}://{IP}:{Port}/uiamrcommand";
        ServiceEndpoint_uitablecommand = $"{header}://{IP}:{Port}/uitablecommand";
    }

    public static void SetContainerIP(bool IsHttps, string IP, string Port)
    {
        string header = IsHttps ? "https" : "http";
        ContainerHeader = header;
        ContainerIP = IP;
        ContainerPort = Port;
        ContainerEndpoint_locationSubPub = $"{header}://{IP}:{Port}/locationSubPub";
        ContainerEndpoint_cancelSubscription = $"{header}://{IP}:{Port}/cancelSubscription";
    }

    public static void SetContainer2IP(bool IsHttps, string IP, string Port)
    {
        string header = IsHttps ? "https" : "http";
        ContainerHeader = header;
        ContainerIP = IP;
        ContainerPort = Port;
        ContainerEndpoint_uicommand = $"{header}://{IP}:{Port}/uicommand";
    }

    public static void SetScanIP(bool IsHttps, string IP, string Port)
    {
        string header = IsHttps ? "https" : "http";
        ScanHeader = header;
        ScanIP = IP;
        ScanPort = Port;
        ScanEndpoint_scan = $"{header}://{IP}:{Port}/scan";
        ScanEndpoint_scanprogress = $"{header}://{IP}:{Port}/scanprogress";
    }
}

public static class CommonParameters
{
    public static string AppName { get; set; } = "Task Manager UI";
}

public static class DebugParameters
{
    // Helper
    public static MessageLevel MsgLvl_HelpCom { get; set; } = MessageLevel.Error;

    // Layout
    public static MessageLevel MsgLvl_LayoutMain { get; set; } = MessageLevel.Error;


    // Models/Common
    public static MessageLevel MsgLvl_SortingModel { get; set; } = MessageLevel.Error;

    // Pages/Account
    public static MessageLevel MsgLvl_AccAdd { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AccEdit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AccLogin { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AccLogout { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AccUser { get; set; } = MessageLevel.Error;

    // Pages/Common
    public static MessageLevel MsgLvl_MsgList { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_PopContainer { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_PopEndProd { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_PopLine { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_PopMsgData { get; set; } = MessageLevel.Error;

    // Pages/Mattel
    public static MessageLevel MsgLvl_Gaylord { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_GaylordPage { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CheckIn { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CheckInPage { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CheckInMaintenance { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CheckInManual { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CheckOutPage { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CPC { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_CPCPage { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_EditOrderKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_Island { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_LayoutList { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_LineEdit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_LineList { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_LineRun { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_LineView { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MoveRobot { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MoveTable { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_OrderKitList { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_Standard { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_SetupLayout { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_SetupOderKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_TableEdit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_VAS { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ViewPort { get; set; } = MessageLevel.Error;

    // Pages/Mattel/Manual
    public static MessageLevel MsgLvl_AddKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AddKitSuccess { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_AddPart { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_EditKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_EditPart { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_SetupKit { get; set; } = MessageLevel.Error;

    // Pages/Mattel/Scan
    public static MessageLevel MsgLvl_Manage { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanAddKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanAddPart { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanAll { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanOne { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanEditKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanEditPart { get; set; } = MessageLevel.Error;

    // Pages/Menu
    public static MessageLevel MsgLvl_MenuKit { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MenuLine { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MenuMain { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MenuMaintenance { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MenuUser { get; set; } = MessageLevel.Error;

    // Pages/Setting
    public static MessageLevel MsgLvl_Settings { get; set; } = MessageLevel.Error;

    // Service/Authentication
    public static MessageLevel MsgLvl_ServCustom { get; set; } = MessageLevel.Error;

    // Service/DR
    public static MessageLevel MsgLvl_ContainerController { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ContainerService { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanController { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ScanService { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_TaskManagerController { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_TaskManagerService { get; set; } = MessageLevel.Error;

    // Service/Info
    public static MessageLevel MsgLvl_InfoController { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_MessageService { get; set; } = MessageLevel.Error;

    // Service/KeepAlive
    public static MessageLevel MsgLvl_ServAliveCont { get; set; } = MessageLevel.Error;
    public static MessageLevel MsgLvl_ServAliveServ { get; set; } = MessageLevel.Error;

    // Service/Print
    public static MessageLevel MsgLvl_PrintService { get; set; } = MessageLevel.Error;

    // Service/Mattel
    public static MessageLevel MsgLvl_MattelService { get; set; } = MessageLevel.Error;

    // Service/RCS
    public static MessageLevel MsgLvl_RCSService { get; set; } = MessageLevel.Error;

    // Service/Setting
    public static MessageLevel MsgLvl_SettingsService { get; set; } = MessageLevel.Error;
}
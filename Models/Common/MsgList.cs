using TaskManagerWeb.Components.Models.Http;

namespace TaskManagerWeb.Components.Models.Common;
public class MsgData
{
    public int ID { get; set; } = 0;
    public string TaskID { get; set; } = string.Empty;
    public DisplayString Title { get; set; } = new();
    public List<DisplayString> TextList { get; set; } = [];
    public Action OnClick { get; set; } = null!;
}

public class DisplayString
{
    public string Text { get; set; } = string.Empty;
    public string Foreground { get; set; } = "#ffffff";
    public string Background { get; set; } = "#000000";
}

public class MsgList
{
    public List<MsgData> List { get; set; } = new();
}

public class MsgGrp
{
    public string AreaID { get; set; } = string.Empty;
    public List<HttpClass.UIMessage> List { get; set; } = new();
}

public class MsgGrpList
{
    public List<MsgGrp> List { get; set; } = [];
}
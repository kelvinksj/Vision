namespace TaskManagerWeb.Components.Models.Common;
public class PopMsgData
{
    public bool ActivatePopup { get; set; } = false;
    public string Text_1 { get; set; } = string.Empty;
    public string Text_2 { get; set; } = string.Empty;
    public string Text_3 { get; set; } = string.Empty;
    public string Middle_Icon { get; set; } = string.Empty;
    public string BtnText_1 { get; set; } = string.Empty;
    public string BtnText_2 { get; set; } = string.Empty;
    public Action BtnClick_1 { get; set; } = null!;
    public Action BtnClick_2 { get; set; } = null!;
    public string jsCommand
    {
        get
        {
            string command = "const elements = document.getElementsByClassName('PopMsgBtn'); ";
            command += "for (let element of elements) { element.blur(); }";
            return command;
        }
    }
}
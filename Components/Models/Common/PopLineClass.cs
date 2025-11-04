namespace TaskManagerWeb.Components.Models.Common;
public class PopLineData
{
    public bool ActivatePopup { get; set; } = false;
    public Action IconClick { get; set; } = null!;
    public string Text_1 { get; set; } = string.Empty;
    public string Text_2 { get; set; } = string.Empty;
    public Action<string> BoxType { get; set; } = null!;
    public string BoxText_1 { get; set; } = string.Empty;
    public string BoxText_2 { get; set; } = string.Empty;
    public string BoxText_3 { get; set; } = string.Empty;
    public string BtnText_1 { get; set; } = string.Empty;
    public string BtnText_2 { get; set; } = string.Empty;
    public string BtnText_3 { get; set; } = string.Empty;
    public string BtnText_4 { get; set; } = string.Empty;
    public Action BtnClick_1 { get; set; } = null!;
    public Action BtnClick_2 { get; set; } = null!;
    public Action BtnClick_3 { get; set; } = null!;
    public Action BtnClick_4 { get; set; } = null!;
}
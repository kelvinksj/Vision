namespace TaskManagerWeb.Components.Models.Common;
public class PopEndProdData
{
    public bool ActivatePopup { get; set; } = false;
    public Action IconClick { get; set; } = null!;
    public string Text_1 { get; set; } = string.Empty;
    public string Text_2 { get; set; } = string.Empty;
    public bool Answer_1 { get; set; } = false;
    public bool Answer_2 { get; set; } = false;
    public bool Answer_3 { get; set; } = false;
    public string BtnText_1 { get; set; } = string.Empty;
    public string BtnText_2 { get; set; } = string.Empty;
    public string BtnText_3 { get; set; } = string.Empty;
    public string BtnText_4 { get; set; } = string.Empty;
    public Action BtnClick_1 { get; set; } = null!;
    public Action BtnClick_2 { get; set; } = null!;
    public Action BtnClick_3 { get; set; } = null!;
    public Action BtnClick_4 { get; set; } = null!;
    public Action<bool, bool, bool> UpdateParent { get; set; } = null!;
}
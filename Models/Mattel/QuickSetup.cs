using Microsoft.AspNetCore.Components;

namespace TaskManagerWeb.Components.Models.Mattel;
public class QuickSetupClass
{
    public bool IsVisible { get; set; } = true;
    public bool ShowInitial { get; set; } = true;
    // Offsets to keep position consistent across pages
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
    public List<DraggableItem> InitialItems = new List<DraggableItem>
    {
        new DraggableItem { CssClass = "vLeft-table", TopPx = "20px", LeftPx = "11px", IsInitial = true },
        new DraggableItem { CssClass = "vRight-table", TopPx = "20px", LeftPx = "81px", IsInitial = true },
        new DraggableItem { CssClass = "shortX-table", TopPx = "114px", LeftPx = "6px", IsInitial = true },
        new DraggableItem { CssClass = "longX-table", TopPx = "114px", LeftPx = "69px", IsInitial = true },
        new DraggableItem { CssClass = "shortY-table", TopPx = "195px", LeftPx = "15px", IsInitial = true },
        new DraggableItem { CssClass = "longY-table", TopPx = "187px", LeftPx = "85px", IsInitial = true },
        new DraggableItem { CssClass = "icon_person01", TopPx = "287px", LeftPx = "9px", IsInitial = true }
    };
    public List<DraggableItem> Items = new List<DraggableItem>();

    public EventCallback Event1 { get; set; } = new();
    public EventCallback Event2 { get; set; } = new();

    public void SetEventCallbacks(
        object Receiver,
        Func<Task> Handler1,
        Func<Task> Handler2
    )
    {
        Event1 = EventCallback.Factory.Create(Receiver, Handler1);
        Event2 = EventCallback.Factory.Create(Receiver, Handler2);
    }

    public class DraggableItem
    {
        public string CssClass { get; set; } = string.Empty;
        public string TopPx { get; set; } = "0px";
        public string LeftPx { get; set; } = "0px";
        public bool IsDraggable { get; set; } = true;
        public bool IsInitial { get; set; } = false; // Identifies if this is an initial item  

        // Get the width of the item based on its CSS class
        public double GetWidth()
        {
            return CssClass switch
            {
                "vLeft-table" => 30,
                "vRight-table" => 30,
                "shortX-table" => 40,
                "longX-table" => 55,
                "shortY-table" => 20,
                "longY-table" => 20,
                "icon_person01" => 50,
                "box-outer" => 60,
                _ => 50 // Default width
            };
        }

        // Get the height of the item based on its CSS class
        public double GetHeight()
        {
            return CssClass switch
            {
                "vLeft-table" => 30,
                "vRight-table" => 30,
                "shortX-table" => 20,
                "longX-table" => 20,
                "shortY-table" => 40,
                "longY-table" => 55,
                "icon_person01" => 100,
                "box-outer" => 60,
                _ => 50 // Default height
            };
        }
    }
}
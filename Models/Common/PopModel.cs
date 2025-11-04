using Microsoft.AspNetCore.Components;

namespace TaskManagerWeb.Components.Models.Common;
public class PopEditData
{
    public bool IsVisible { get; set; } = false;
    public string Header { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public EventCallback EventOk { get; set; } = new();
    public EventCallback EventNok { get; set; } = new();

    public void SetEventCallbacks(
        object receiver,
        Func<Task> confirmHandler,
        Func<Task> cancelHandler
    )
    {
        EventOk = EventCallback.Factory.Create(receiver, confirmHandler);
        EventNok = EventCallback.Factory.Create(receiver, cancelHandler);
    }
}

public class PopHeaderData
{
    public bool IsVisible { get; set; } = false;
    public string Header { get; set; } = string.Empty;
    public EventCallback EventOk { get; set; } = new();
    public EventCallback EventNok { get; set; } = new();

    public void SetEventCallbacks(
        object receiver,
        Func<Task> confirmHandler,
        Func<Task> cancelHandler
    )
    {
        EventOk = EventCallback.Factory.Create(receiver, confirmHandler);
        EventNok = EventCallback.Factory.Create(receiver, cancelHandler);
    }
}

public class PopTableEditData : PopHeaderData
{
    public Element.Table Table { get; set; } = new();
}
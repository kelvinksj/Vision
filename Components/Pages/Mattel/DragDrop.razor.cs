using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Mattel;
using static TaskManagerWeb.Components.Models.Mattel.QuickSetupClass;

namespace TaskManagerWeb.Components.Pages.Mattel;
public partial class DragDrop : IDisposable
{
    [Parameter]
    public QuickSetupClass QuickSetupData { get; set; } = new();

    private string taskName = "DragDrop";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_DragDrop;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private IDisposable? registration;
    private ClaimsPrincipal authUser = new();
    private string? currentUserName = string.Empty;
    private DraggableItem? draggedItem;
    private double mouseXOffset;
    private double mouseYOffset;
    private double lastLeft;
    private double lastTop;

    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

        UpdateInfoChange();

        // Subscribe to URL customize LocationChanged event
        NavigationManager.LocationChanged += OnLocationChanged;

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitializedAsync triggered",
            settingLevel,
            MessageLevel.Information
        );
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        authUser = authState.User;
        currentUserName = authUser.Identity!.Name;

        if (!authUser.IsInRole("Administrator")
        && !authUser.IsInRole("Engineer")
        && !authUser.IsInRole("Supervisor"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        // CommonLib.DisplayConsole(
        //     taskName,
        //     $"OnAfterRenderAsync triggered",
        //     settingLevel,
        //     MessageLevel.Information
        // );

        if (FirstRender && urlPage == taskName)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnAfterRenderAsync first rendor triggered",
                settingLevel,
                MessageLevel.Information
            );

            try
            {
                registration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);
            }
            catch (Exception ex)
            {
                CommonLib.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }
        }

        await base.OnAfterRenderAsync(FirstRender);
    }

    public void Dispose()
    // Need to add "@implements IDisposable" at the top of the razor file
    // and add ": IDisposable" after partial class on the same line
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Dispose triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    protected async virtual void DisposeAsync(bool Disposing)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"DisposeAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (!disposed)
        {
            if (Disposing)
            // Cleanup managed resources
            {
                try
                {
                    // Unsubscribe customize LocationChanged event from URL
                    NavigationManager.LocationChanged -= OnLocationChanged;
                    registration?.Dispose();
                }
                catch (Exception ex)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        ex,
                        settingLevel,
                        MessageLevel.Error
                    );
                }
            }

            // Cleanup unmanaged resources (if any)
            disposed = true;
        }

        await Task.CompletedTask;
    }

    // Finalizer (destructor)
    ~DragDrop()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    private ValueTask OnLocationChanging(LocationChangingContext context)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnLocationChanging triggered",
            settingLevel,
            MessageLevel.Information
        );
        // context.PreventNavigation();
        return ValueTask.CompletedTask;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    // Handle the URL change here (e.g., update styles, perform actions, etc.)
    {
        await UpdateInfoChange();
        await InvokeAsync(StateHasChanged);
    }

    private Task UpdateInfoChange()
    // Update all the necessary variable or task for the page when URL change
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateInfoChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            currentUrl = NavigationManager.Uri;
            urlPage = CommonLib.GetPageName(currentUrl, 0);

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange: urlPage={urlPage}, taskName={taskName}",
                settingLevel,
                MessageLevel.Trace
            );

            if (urlPage != taskName)
                return Task.CompletedTask;

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange done",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    private bool IsInitialItem(DraggableItem item)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"IsInitialItem triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {

        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return item.IsInitial;
    }

    private async void DragStart(DragEventArgs e, DraggableItem item)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"DragStart triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            Console.WriteLine("DragStart triggered");
            draggedItem = item;

            // Store the last known position before dragging
            lastLeft = Convert.ToDouble(item.LeftPx.Replace("px", ""));
            lastTop = Convert.ToDouble(item.TopPx.Replace("px", ""));

            mouseXOffset = e.ClientX - lastLeft;
            mouseYOffset = e.ClientY - lastTop;

            // mouseXOffset = e.ClientX - Convert.ToDouble(item.LeftPx.Replace("px", ""));
            // mouseYOffset = e.ClientY - Convert.ToDouble(item.TopPx.Replace("px", ""));

            await jsRuntime.InvokeVoidAsync("setDragData", item);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void HandleOnMouseDown()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"HandleOnMouseDown triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            await JsInteropHelper.RemoveAllRangesAsync(jsRuntime);
            Console.WriteLine("MouseDown");
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void Drop(DragEventArgs e)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Drop triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            Console.WriteLine($"Drop triggered at X: {e.ClientX}, Y: {e.ClientY}");

            if (draggedItem != null)
            {
                bool isInsideDropArea = await jsRuntime.InvokeAsync<bool>("checkDropTarget", e.ClientX, e.ClientY);

                // Calculate new position based on drag
                var adjustedX = e.ClientX - mouseXOffset;
                var adjustedY = e.ClientY - mouseYOffset;

                Console.WriteLine($"Adjusted Drop Position: X = {adjustedX}px, Y = {adjustedY}px");

                // Check if the item was dropped inside the delete area
                int deleteIconX = -10; // Left position of the icon_delete
                int deleteIconY = -95; // Top position of the icon_delete
                int deleteIconWidth = 60; // Width of the delete box
                int deleteIconHeight = 60; // Height of the delete box

                Console.WriteLine($"Delete Icon Area: X = {deleteIconX} - {deleteIconX + deleteIconWidth}, " +
                              $"Y = {deleteIconY} - {deleteIconY + deleteIconHeight}");

                // Check if the item was dropped within the delete box
                if (adjustedX >= deleteIconX && adjustedX <= (deleteIconX + deleteIconWidth) &&
                adjustedY >= deleteIconY && adjustedY <= (deleteIconY + deleteIconHeight))
                {
                    if (!IsInitialItem(draggedItem))
                    {
                        Console.WriteLine("Item deleted");
                        QuickSetupData.Items.Remove(draggedItem);
                        StateHasChanged();
                    }
                }
                else
                {
                    if (isInsideDropArea)
                    {
                        if (!IsOverlapping(adjustedX, adjustedY, draggedItem))
                        {
                            if (IsInitialItem(draggedItem))
                            {
                                // Duplicate the initial item
                                var items = new DraggableItem
                                {
                                    CssClass = draggedItem.CssClass,
                                    TopPx = $"{adjustedY}px",
                                    LeftPx = $"{adjustedX}px",
                                    IsDraggable = true,
                                    IsInitial = false
                                };
                                QuickSetupData.Items.Add(items);
                            }
                            else
                            {
                                Console.WriteLine("Drop initial item");
                                // Move the already dropped item
                                draggedItem.TopPx = $"{adjustedY}px";
                                draggedItem.LeftPx = $"{adjustedX}px";
                            }
                        }
                        else
                        {
                            Console.WriteLine("Overlap detected! Moving back to last position.");
                            // Reset item if it overlaps
                            if (!IsInitialItem(draggedItem))
                            {
                                Console.WriteLine("Item from initial list removed due to overlap");
                                // QuickSetupData.Items.Remove(draggedItem);
                            }
                            else
                            {
                                // Move back to the last known position
                                draggedItem.TopPx = $"{lastTop}px";
                                draggedItem.LeftPx = $"{lastLeft}px";
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Drop rejected");
                    }

                    // Clear the dragged item reference
                    draggedItem = null;

                    // Force UI refresh
                    StateHasChanged();

                    Console.WriteLine($"Total item: {QuickSetupData.Items.Count}");
                }
            }
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    // private bool IsOverlapping(double x, double y, DraggableItem currentItem)
    // {
    //     CommonLib.DisplayConsole(
    //         taskName,
    //         $"IsOverlapping triggered",
    //         settingLevel,
    //         MessageLevel.Information
    //     );

    //     double currentWidth = currentItem.GetWidth();
    //     double currentHeight = currentItem.GetHeight();

    //     try
    //     {
    //         foreach (var item in QuickSetupData.Items)
    //         {
    //             if (item == currentItem) continue; // Skip self-check

    //             double itemX = Convert.ToDouble(item.LeftPx.Replace("px", ""));
    //             double itemY = Convert.ToDouble(item.TopPx.Replace("px", ""));
    //             double itemWidth = item.GetWidth();
    //             double itemHeight = item.GetHeight();

    //             // Bounding box collision check
    //             bool isOverlapping = x < itemX + itemWidth &&
    //                                  x + currentWidth > itemX &&
    //                                  y < itemY + itemHeight &&
    //                                  y + currentHeight > itemY;

    //             if (isOverlapping)
    //             {
    //                 return true; // Overlap detected
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         CommonLib.DisplayConsole(
    //             taskName,
    //             ex,
    //             settingLevel,
    //             MessageLevel.Error
    //         );
    //     }

    //     return false; // No overlap
    // }

    private bool IsOverlapping(double x, double y, DraggableItem currentItem)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"IsOverlapping triggered",
            settingLevel,
            MessageLevel.Information
        );

        double currentWidth = currentItem.GetWidth();
        double currentHeight = currentItem.GetHeight();

        try
        {
            foreach (var item in QuickSetupData.Items)
            {
                if (item == currentItem) continue; // Skip self-check

                double itemX = Convert.ToDouble(item.LeftPx.Replace("px", ""));
                double itemY = Convert.ToDouble(item.TopPx.Replace("px", ""));
                double itemWidth = item.GetWidth();
                double itemHeight = item.GetHeight();

                // Bounding box collision check (allows full overlap)
                bool isOverlapping = x < itemX + itemWidth &&
                                     x + currentWidth > itemX &&
                                     y < itemY + itemHeight &&
                                     y + currentHeight > itemY;

                if (isOverlapping)
                {
                    return false; // Overlapping is allowed
                }
            }
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return false; // Always allow placement
    }
}
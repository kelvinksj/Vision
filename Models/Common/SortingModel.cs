using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Models.Common;
public enum SortState
{
    Default,
    Ascending,
    Descending
}

public class SortingModel<T>
{
    public string taskName = "SortingModel";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_SortingModel;
    private CommonLib commonLib = new();
    public Dictionary<string, SortState> SortStates { get; set; } = new Dictionary<string, SortState>();
    public Dictionary<string, string> SortIcons { get; set; } = new Dictionary<string, string>();

    public SortingModel(IEnumerable<string> columns)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SortingModel triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            foreach (var column in columns)
            {
                SortStates[column] = SortState.Default;
                SortIcons[column] = "icon_filter_darkGrey";
            }
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
    }

    public List<T> SortItems(List<T> items, string column)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SortItems triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Reset all columns to default state
            foreach (var key in SortStates.Keys.ToList())
            {
                if (key != column)
                {
                    SortStates[key] = SortState.Default;
                    SortIcons[key] = "icon_filter_darkGrey";  // Set default icon for all columns
                }
            }

            // Update the sort state for the selected column
            switch (SortStates[column])
            {
                case SortState.Default:
                    SortStates[column] = SortState.Ascending;
                    items = items.OrderBy(i => GetPropertyValue(i, column)).ToList();
                    SortIcons[column] = "icon_sortAsc_darkGrey";  // Set ascending icon
                    break;

                case SortState.Ascending:
                    SortStates[column] = SortState.Descending;
                    items = items.OrderByDescending(i => GetPropertyValue(i, column)).ToList();
                    SortIcons[column] = "icon_sortDesc_darkGrey";  // Set descending icon
                    break;

                case SortState.Descending:
                    SortStates[column] = SortState.Ascending;  // Cycle back to ascending
                    items = items.OrderBy(i => GetPropertyValue(i, column)).ToList();
                    SortIcons[column] = "icon_sortAsc_darkGrey";  // Set ascending icon
                    break;
            }

            // Log sorted items
            commonLib.DisplayConsole(
                taskName,
                $"SortItems: Sorted Items:",
                settingLevel,
                MessageLevel.Trace
            );
            commonLib.DisplayConsole(
                taskName,
                $"SortItems: {commonLib.JsonSerialize(items)}",
                settingLevel,
                MessageLevel.Trace
            );
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

        return items;
    }

    public async Task<List<T>> SortItemsAsync(List<T> items, string column)
    {
        commonLib.DisplayConsole(
            taskName,
            $"SortItemsAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Reset all columns to default state
            foreach (var key in SortStates.Keys.ToList())
            {
                if (key != column)
                {
                    SortStates[key] = SortState.Default;
                    SortIcons[key] = "icon_filter_darkGrey";  // Set default icon for all columns
                }
            }

            // Update the sort state for the selected column
            switch (SortStates[column])
            {
                case SortState.Default:
                    SortStates[column] = SortState.Ascending;
                    items = [.. items.OrderBy(i => GetPropertyValue(i, column))];
                    SortIcons[column] = "icon_sortAsc_darkGrey";  // Set ascending icon
                    break;

                case SortState.Ascending:
                    SortStates[column] = SortState.Descending;
                    items = [.. items.OrderByDescending(i => GetPropertyValue(i, column))];
                    SortIcons[column] = "icon_sortDesc_darkGrey";  // Set descending icon
                    break;

                case SortState.Descending:
                    SortStates[column] = SortState.Ascending;  // Cycle back to ascending
                    items = [.. items.OrderBy(i => GetPropertyValue(i, column))];
                    SortIcons[column] = "icon_sortAsc_darkGrey";  // Set ascending icon
                    break;
            }

            // Log sorted items
            commonLib.DisplayConsole(
                taskName,
                $"SortItemsAsync: Sorted Items:",
                settingLevel,
                MessageLevel.Trace
            );
            commonLib.DisplayConsole(
                taskName,
                $"SortItemsAsync: {commonLib.JsonSerialize(items)}",
                settingLevel,
                MessageLevel.Trace
            );
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

        return await Task.FromResult(items);
    }

    public async Task<List<T>> ReSortItemsAsync(List<T> items)
    {
        commonLib.DisplayConsole(
            taskName,
            $"ReSortItemsAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Reset all columns to default state
            foreach (var key in SortStates.Keys.ToList())
            {
                commonLib.DisplayConsole(
                    taskName,
                    $"ReSortItemsAsync: key={key}",
                    settingLevel,
                    MessageLevel.Trace
                );
                commonLib.DisplayConsole(
                    taskName,
                    $"ReSortItemsAsync: SortIcons={SortStates[key]}",
                    settingLevel,
                    MessageLevel.Trace
                );

                if (SortStates[key] == SortState.Ascending
                    || SortStates[key] == SortState.Descending
                )
                {
                    // Update the sort state for the selected column
                    switch (SortStates[key])
                    {
                        case SortState.Default:
                        case SortState.Ascending:
                            items = [.. items.OrderBy(i => GetPropertyValue(i, key))];
                            break;

                        case SortState.Descending:
                            items = [.. items.OrderByDescending(i => GetPropertyValue(i, key))];
                            break;
                    }

                    break;
                }
            }
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

        return await Task.FromResult(items);
    }

    private object GetPropertyValue(T item, string propertyName)
    {
        commonLib.DisplayConsole(
            taskName,
            $"GetPropertyValue triggered",
            settingLevel,
            MessageLevel.Information
        );

        string[] propertyParts = propertyName.Split('.');
        object value = item!;

        foreach (var part in propertyParts)
        {
            if (value == null)
            {
                throw new NullReferenceException($"Value of {propertyName} is null on {typeof(T).Name}");
            }

            var property = value.GetType().GetProperty(part);

            if (property == null)
            {
                throw new ArgumentException($"Property {part} not found on {value.GetType().Name}");
            }

            value = property.GetValue(value)!;
        }

        commonLib.DisplayConsole(
            taskName,
            $"GetPropertyValue: Property: {propertyName}, Value: {value}",
            settingLevel,
            MessageLevel.Trace
        );

        return value ?? throw new NullReferenceException($"Value of {propertyName} is null on {typeof(T).Name}");
    }
}
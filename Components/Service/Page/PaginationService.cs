namespace TaskManagerWeb.Components.Service.Page;
public class PaginationService<T>
{
    private List<T> items;
    public int CurrentPage { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 5;

    public PaginationService(IEnumerable<T> Items)
    {
        items = Items.ToList();
    }

    public void SetItems(IEnumerable<T> Items)
    {
        items = Items.ToList();
        HandleEmptyPage();
    }

    public IEnumerable<T> GetItems()
    {
        return items.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage);
    }

    public int GetTotalPages()
    {
        return (int)Math.Ceiling((double)items.Count / ItemsPerPage);
    }

    public void GoToPage(int Page)
    {
        if (Page > 0 && Page <= GetTotalPages())
        {
            CurrentPage = Page;
        }
    }

    private void HandleEmptyPage()
    {
        if (!items.Any())
        {
            CurrentPage = 1; // Reset to the first page if no items
        }
        else if (CurrentPage > GetTotalPages())
        {
            CurrentPage = GetTotalPages(); // Navigate to the last valid page
        }
    }

    public void GoPageNumber(int recordIndex)
    {
        if (recordIndex < 0 || recordIndex >= items.Count)
            CurrentPage = 1;
        else
            // Calculate the page number (1-based index)
            CurrentPage = (recordIndex / ItemsPerPage) + 1;
    }

    public int FindRecordIndexByClassInstance(
        T comparisonInstance,
        List<string> variableNames
    )
    {
        if (comparisonInstance == null
            || variableNames == null
            || variableNames.Count == 0
        )
            return -1;

        return (items ?? [])
            .Select((item, index) => new { item, index })
            .FirstOrDefault(n =>
            {
                var dynamicItem = (dynamic)n.item!;
                var comparisonDynamic = (dynamic)comparisonInstance;

                foreach (var variableName in variableNames)
                {
                    var propertyInfo = dynamicItem
                        .GetType()
                        .GetProperty(variableName);

                    if (propertyInfo != null)
                    {
                        var propertyValue = propertyInfo
                            .GetValue(dynamicItem)?
                            .ToString();
                        var comparisonValue = propertyInfo
                            .GetValue(comparisonDynamic)?
                            .ToString();

                        if (propertyValue == comparisonValue)
                            return true;
                    }
                }

                return false;
            })?.index ?? -1;
    }
}

namespace NotebookDB.Common
{
    public class Search
    {
        public string? FilterText { get; set; }
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 15;
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    }

    public enum SortDirection
    {
        Descending = -1,
        Ascending = 1
    }

    public class SearchResponse<T>
    {
        public List<T> Results { get; set; } = new List<T>();
        public int Total { get; set; }
    }

    public class ListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class TemplateListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}

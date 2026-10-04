namespace CarRent.Application.Models.Pagination;

public class PagedList<T>
{
    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public bool HasPrevious => CurrentPage > 1;

    public bool HasNext => CurrentPage < TotalPages;

    public List<T> Items { get; set; } = new();
    
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
}
namespace Back.Interfaces;

public class PageResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPage;
    private PageResult() { }
    public static PageResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PageResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}

// var result = PageResult<OfficeDto>.Create(offices, totalCount, pageNumber, pageSize);

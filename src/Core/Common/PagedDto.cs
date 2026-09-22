namespace Core.Common
{
    public class PagedDto<TEntity> where TEntity : class
    {
        public int TotalRecords { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public TEntity Data { get; set; } = default!;
    }
}
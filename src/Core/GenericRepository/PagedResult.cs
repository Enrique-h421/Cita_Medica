using System.Collections.Generic;

namespace Core.GenericRepository
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
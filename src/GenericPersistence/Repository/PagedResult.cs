using System;
using System.Collections.Generic;

namespace GenericPersistence.Repository
{
    public class PagedResult<TEntity>
    {
        public IEnumerable<TEntity> Data { get; set; } = new List<TEntity>();
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalRecords / (double)PageSize) : 0;
    }
}
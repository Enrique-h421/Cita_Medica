using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GenericPersistence.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> ObtenerTodosAsync();
        Task<T?> ObtenerPorIdAsync(object id);
        Task CrearAsync(T entidad);
        Task ActualizarAsync(T entidad);
        Task EliminarAsync(object id);

        Task<T?> GetOneByAsync(Expression<Func<T, bool>> filter);

        Task CrearRangoAsync(IEnumerable<T> entidades);

        Task<PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? orderByField = null,
            bool asNoTracking = true,
            bool splitQuery = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes);
    }
}
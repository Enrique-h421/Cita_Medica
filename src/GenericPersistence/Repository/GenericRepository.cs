using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericPersistence.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> ObtenerPorIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task CrearAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(T entidad)
        {
            _dbSet.Update(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(object id)
        {
            var entidad = await _dbSet.FindAsync(id);
            if (entidad != null)
            {
                _dbSet.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<T?> GetOneByAsync(Expression<Func<T, bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public async Task CrearRangoAsync(IEnumerable<T> entidades)
        {
            if (entidades == null) throw new ArgumentNullException(nameof(entidades));
            await _dbSet.AddRangeAsync(entidades);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? orderByField = null,
            bool asNoTracking = true,
            bool splitQuery = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be 1 or greater.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be 1 or greater.");

            var query = ApplyIncludes(Query(asNoTracking), includes);
            if (filter != null) query = query.Where(filter);
            if (splitQuery) query = query.AsSplitQuery();

            int totalRecords = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(orderByField))
            {
                query = query.OrderBy(orderByField);
            }
            else if (orderBy != null)
            {
                query = orderBy(query);
            }

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var data = await query.ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        private IQueryable<T> Query(bool asNoTracking) =>
            asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        private static IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Core.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace Persistence.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ClinicaDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ClinicaDbContext context)
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

        public async Task<Core.GenericRepository.PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true,
            bool splitQuery = false,
            CancellationToken cancellationToken = default,
            string? orderByField = null,
            params Expression<Func<T, object>>[] includes)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be 1 or greater.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be 1 or greater.");

            var query = ApplyIncludes(Query(asNoTracking), includes);
            if (filter != null) query = query.Where(filter);
            if (splitQuery) query = query.AsSplitQuery();

            // Count against the filtered (but not yet ordered/paged) query — computed
            // with CountAsync so it doesn't block a thread pool thread synchronously.
            int totalRecords = await query.CountAsync(cancellationToken);

            // Punto 2 del enunciado: OrderBy dinámico por cualquier campo (ej: "FechaHoraInicio desc")
            if (!string.IsNullOrWhiteSpace(orderByField))
                query = query.OrderBy(orderByField);
            else if (orderBy != null)
                query = orderBy(query);

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var data = await query.ToListAsync(cancellationToken);

            return new Core.GenericRepository.PagedResult<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        // Punto 1 del enunciado: GetOneBy genérico con filtrado genérico
        public async Task<T?> GetOneByAsync(
            Expression<Func<T, bool>> filter,
            bool asNoTracking = true,
            params Expression<Func<T, object>>[] includes)
        {
            var query = ApplyIncludes(Query(asNoTracking), includes);
            return await query.FirstOrDefaultAsync(filter);
        }

        // Punto 3 del enunciado: guardar una lista de objetos (rango de valores)
        public async Task CrearRangoAsync(IEnumerable<T> entidades)
        {
            await _dbSet.AddRangeAsync(entidades);
            await _context.SaveChangesAsync();
        }

        private IQueryable<T> Query(bool asNoTracking = true)
        {
            return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        }

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
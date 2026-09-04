using System.Collections.Generic;
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
    }
}
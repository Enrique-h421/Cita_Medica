using System.Collections.Generic;
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
    }
}
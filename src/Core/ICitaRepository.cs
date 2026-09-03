using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.AgendaYAtencion;

namespace Core
{
    public interface ICitaRepository
    {
        Task<IEnumerable<Cita>> ObtenerTodasLasCitasAsync();
        Task<Cita?> ObtenerCitaPorIdAsync(long id);
        Task CrearCitaAsync(Cita cita);
        Task ActualizarCitaAsync(Cita cita);
        Task EliminarCitaAsync(long id);
    }
}
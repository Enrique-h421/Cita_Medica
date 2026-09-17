using Core;
using Domain.AgendaYAtencion;
using GenericPersistence.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.repositories
{
    public class CitaRepository : ICitaRepository
    {
        private readonly ClinicaDbContext _context;
        private readonly IGenericRepository<Cita> _genericRepository;

        public CitaRepository(ClinicaDbContext context, IGenericRepository<Cita> genericRepository)
        {
            _context = context;
            _genericRepository = genericRepository;
        }

        public async Task<IEnumerable<Cita>> ObtenerTodasLasCitasAsync()
        {
            return await _context.Citas
                                 .Include(c => c.EstadoCita)
                                 .ToListAsync();
        }

        public async Task<Cita?> ObtenerCitaPorIdAsync(long id)
        {
            return await _context.Citas
                                 .Include(c => c.EstadoCita)
                                 .FirstOrDefaultAsync(c => c.CitaID == id);
        }

        public async Task CrearCitaAsync(Cita cita)
        {
            await _genericRepository.CrearAsync(cita);
        }

        public async Task ActualizarCitaAsync(Cita cita)
        {
            await _genericRepository.ActualizarAsync(cita);
        }

        public async Task EliminarCitaAsync(long id)
        {
            await _genericRepository.EliminarAsync(id);
        }
    }
}
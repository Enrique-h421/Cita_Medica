using Core;
using Domain.AgendaYAtencion;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.repositories
{
    public class CitaRepository : ICitaRepository
    {
        private readonly ClinicaDbContext _context;

        public CitaRepository(ClinicaDbContext context)
        {
            _context = context;
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
            await _context.Citas.AddAsync(cita);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarCitaAsync(Cita cita)
        {
            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarCitaAsync(long id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();
            }
        }
    }
}
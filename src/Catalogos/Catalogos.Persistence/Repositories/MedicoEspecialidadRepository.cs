using Catalogos.Core.Repositories;
using Catalogos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogos.Persistence.Repositories
{
    public class MedicoEspecialidadRepository : IMedicoEspecialidadRepository
    {
        private readonly CatalogosDbContext _context;

        public MedicoEspecialidadRepository(CatalogosDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicoEspecialidad>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
            => await _context.MedicoEspecialidades.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<List<MedicoEspecialidad>> ObtenerPorMedicoAsync(
            int medicoId, CancellationToken cancellationToken = default)
            => await _context.MedicoEspecialidades
                             .AsNoTracking()
                             .Where(r => r.MedicoID == medicoId)
                             .ToListAsync(cancellationToken);

        public async Task AsignarAsync(
            int medicoId,
            IEnumerable<int> especialidadIds,
            int? especialidadPrincipalId,
            CancellationToken cancellationToken = default)
        {
            var deseadas = especialidadIds.Distinct().ToList();

            var actuales = await _context.MedicoEspecialidades
                                         .Where(r => r.MedicoID == medicoId)
                                         .ToListAsync(cancellationToken);

            // Quitar las que ya no van.
            var sobrantes = actuales.Where(r => !deseadas.Contains(r.EspecialidadID)).ToList();

            if (sobrantes.Count > 0)
            {
                _context.MedicoEspecialidades.RemoveRange(sobrantes);
            }

            // Agregar las nuevas y ajustar la bandera de principal en las que ya estaban.
            foreach (var id in deseadas)
            {
                var existente = actuales.FirstOrDefault(r => r.EspecialidadID == id);
                var esPrincipal = especialidadPrincipalId == id;

                if (existente is null)
                {
                    _context.MedicoEspecialidades.Add(new MedicoEspecialidad
                    {
                        MedicoID = medicoId,
                        EspecialidadID = id,
                        Principal = esPrincipal
                    });
                }
                else if (existente.Principal != esPrincipal)
                {
                    existente.Principal = esPrincipal;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task EliminarPorMedicoAsync(int medicoId, CancellationToken cancellationToken = default)
        {
            var suyas = await _context.MedicoEspecialidades
                                      .Where(r => r.MedicoID == medicoId)
                                      .ToListAsync(cancellationToken);

            if (suyas.Count == 0) return;

            _context.MedicoEspecialidades.RemoveRange(suyas);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.AgendaYAtencion; 

namespace Core.Queries
{
    public class ObtenerCitasQuery : IRequest<List<Cita>>
    {
    }

    public class ObtenerCitasHandler : IRequestHandler<ObtenerCitasQuery, List<Cita>>
    {
        private readonly ICitaRepository _repository;

        public ObtenerCitasHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Cita>> Handle(ObtenerCitasQuery request, CancellationToken cancellationToken)
        {
           
            var citas = await _repository.ObtenerTodasLasCitasAsync();
            return citas.ToList();
        }
    }
}
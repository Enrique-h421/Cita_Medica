using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Domain.AgendaYAtencion;

namespace Core.Commands
{
  
    public class CrearCitaCommand : IRequest<bool>
    {
        public Cita NuevaCita { get; set; }
    }

    public class CrearCitaHandler : IRequestHandler<CrearCitaCommand, bool>
    {
        private readonly ICitaRepository _repository;

        public CrearCitaHandler(ICitaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CrearCitaCommand request, CancellationToken cancellationToken)
        {
            await _repository.CrearCitaAsync(request.NuevaCita);
            return true; 
        }
    }
}
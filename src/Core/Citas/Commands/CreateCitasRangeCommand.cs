using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Citas.Dtos;
using Core.Common;
using Core.GenericRepository;
using Domain.AgendaYAtencion;
using MediatR;

namespace Core.Citas.Commands
{
    public class CreateCitasRangeCommand : IRequest<HttpResponse<bool>>
    {
        public List<CreateCitaDto> Citas { get; set; } = new();
    }

    public class CreateCitasRangeCommandHandler : IRequestHandler<CreateCitasRangeCommand, HttpResponse<bool>>
    {
        private readonly IGenericRepository<Cita> _repository;
        private readonly IMapper _mapper;

        public CreateCitasRangeCommandHandler(IGenericRepository<Cita> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<HttpResponse<bool>> Handle(CreateCitasRangeCommand request, CancellationToken cancellationToken)
        {
            var entidades = _mapper.Map<List<Cita>>(request.Citas);
            await _repository.CrearRangoAsync(entidades);
            return new HttpResponse<bool>(true);
        }
    }
}
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Citas.Dtos;
using Core.Common;
using Core.GenericRepository;
using Domain.AgendaYAtencion;
using MediatR;

namespace Core.Citas.Queries
{
    public class GetCitaByFilterQuery : IRequest<HttpResponse<GetCitaDto?>>
    {
        public string Filter { get; set; } = string.Empty;
    }

    public class GetCitaByFilterQueryHandler : IRequestHandler<GetCitaByFilterQuery, HttpResponse<GetCitaDto?>>
    {
        private readonly IGenericRepository<Cita> _repository;
        private readonly IMapper _mapper;

        public GetCitaByFilterQueryHandler(IGenericRepository<Cita> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<HttpResponse<GetCitaDto?>> Handle(GetCitaByFilterQuery request, CancellationToken cancellationToken)
        {
            var filterExpression = Filter.FromStringExpression<Cita>(request.Filter);
            var cita = await _repository.GetOneByAsync(filterExpression, includes: c => c.EstadoCita!);
            return new HttpResponse<GetCitaDto?>(_mapper.Map<GetCitaDto?>(cita));
        }
    }
}
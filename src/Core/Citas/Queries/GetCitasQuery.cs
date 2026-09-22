using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class GetCitasQuery : RequestParametersGets, IRequest<HttpResponse<PagedDto<List<GetCitaDto>>>>
    {
        public bool IncludeEstadoCita { get; set; }
        public string? OrderByField { get; set; }
    }

    public class GetCitasQueryEventHandler : IRequestHandler<GetCitasQuery, HttpResponse<PagedDto<List<GetCitaDto>>>>
    {
        private readonly IGenericRepository<Cita> _repository;
        private readonly IMapper _mapper;

        public GetCitasQueryEventHandler(IGenericRepository<Cita> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<HttpResponse<PagedDto<List<GetCitaDto>>>> Handle(GetCitasQuery request, CancellationToken cancellationToken)
        {
            var includes = request.IncludeEstadoCita
                ? new Expression<Func<Cita, object>>[] { c => c.EstadoCita! }
                : Array.Empty<Expression<Func<Cita, object>>>();

            var result = await _repository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                filter: !string.IsNullOrEmpty(request.Filter) ? Filter.FromStringExpression<Cita>(request.Filter) : null,
                orderByField: request.OrderByField,
                includes: includes);

            return new HttpResponse<PagedDto<List<GetCitaDto>>>(new PagedDto<List<GetCitaDto>>
            {
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPage = (int)Math.Ceiling((double)result.TotalRecords / request.PageSize),
                Data = _mapper.Map<List<GetCitaDto>>(result.Data.ToList())
            });
        }
    }
}
using Core.Citas.Commands;
using Core.Citas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CitasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/citas?pageNumber=1&pageSize=10&filter=EstadoCitaID==1&orderByField=FechaHoraInicio desc&includeEstadoCita=true
        [HttpGet]
        public async Task<IActionResult> GetCitas([FromQuery] GetCitasQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // GET: api/citas/buscar?filter=CitaID==5
        [HttpGet("buscar")]
        public async Task<IActionResult> GetCitaByFilter([FromQuery] GetCitaByFilterQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // POST: api/citas/rango
        [HttpPost("rango")]
        public async Task<IActionResult> CreateCitasRange([FromBody] CreateCitasRangeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
using Catalogos.Core.Dtos;
using Catalogos.Core.Features.Pacientes;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PacientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PacientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PacienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerPacientesQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<PacienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerPacientesPaginadosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(long id)
            => Ok(await _mediator.Send(new ObtenerPacientePorIdQuery { PacienteId = id }));

        [HttpPost]
        [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CrearPacienteDto paciente)
        {
            var creado = await _mediator.Send(new CrearPacienteCommand { Paciente = paciente });
            return CreatedAtAction(nameof(GetPorId), new { id = creado.PacienteId }, creado);
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(long id, [FromBody] ActualizarPacienteDto paciente)
        {
            await _mediator.Send(new ActualizarPacienteCommand { PacienteId = id, Paciente = paciente });
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            await _mediator.Send(new EliminarPacienteCommand { PacienteId = id });
            return NoContent();
        }
    }
}

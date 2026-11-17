using Catalogos.Core.Dtos;
using Catalogos.Core.Features.Medicos;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MedicosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<MedicoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerMedicosQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<MedicoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerMedicosPaginadosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(int id)
            => Ok(await _mediator.Send(new ObtenerMedicoPorIdQuery { MedicoId = id }));

        [HttpPost]
        [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CrearMedicoDto medico)
        {
            var creado = await _mediator.Send(new CrearMedicoCommand { Medico = medico });
            return CreatedAtAction(nameof(GetPorId), new { id = creado.MedicoId }, creado);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] ActualizarMedicoDto medico)
        {
            await _mediator.Send(new ActualizarMedicoCommand { MedicoId = id, Medico = medico });
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new EliminarMedicoCommand { MedicoId = id });
            return NoContent();
        }
    }
}

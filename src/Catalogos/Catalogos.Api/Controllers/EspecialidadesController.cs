using Catalogos.Core.Dtos;
using Catalogos.Core.Features.Especialidades;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EspecialidadesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EspecialidadesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EspecialidadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodas()
            => Ok(await _mediator.Send(new ObtenerEspecialidadesQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<EspecialidadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerEspecialidadesPaginadasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EspecialidadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(int id)
            => Ok(await _mediator.Send(new ObtenerEspecialidadPorIdQuery { EspecialidadId = id }));

        [HttpPost]
        [ProducesResponseType(typeof(EspecialidadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CrearEspecialidadDto especialidad)
        {
            var creada = await _mediator.Send(new CrearEspecialidadCommand { Especialidad = especialidad });
            return CreatedAtAction(nameof(GetPorId), new { id = creada.EspecialidadId }, creada);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] ActualizarEspecialidadDto especialidad)
        {
            await _mediator.Send(new ActualizarEspecialidadCommand
            {
                EspecialidadId = id,
                Especialidad = especialidad
            });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new EliminarEspecialidadCommand { EspecialidadId = id });
            return NoContent();
        }
    }
}

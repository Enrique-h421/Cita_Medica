using Core.Dtos;
using Core.Features.Consultorios;
using Domain.Seguridad;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Consultorios de la clínica. Consultarlos lo necesita cualquiera que
    /// agende o atienda; darlos de alta o de baja es configuración del sistema,
    /// y eso es del administrador.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConsultoriosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsultoriosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ConsultorioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerConsultoriosQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<ConsultorioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerConsultoriosPaginadosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ConsultorioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(int id)
            => Ok(await _mediator.Send(new ObtenerConsultorioPorIdQuery { ConsultorioId = id }));

        [HttpPost]
        [Authorize(Roles = RolesSistema.Administrador)]
        [ProducesResponseType(typeof(ConsultorioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Post([FromBody] CrearConsultorioDto consultorio)
        {
            var creado = await _mediator.Send(new CrearConsultorioCommand { Consultorio = consultorio });
            return CreatedAtAction(nameof(GetPorId), new { id = creado.ConsultorioId }, creado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesSistema.Administrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] ActualizarConsultorioDto consultorio)
        {
            await _mediator.Send(new ActualizarConsultorioCommand
            {
                ConsultorioId = id,
                Consultorio = consultorio
            });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesSistema.Administrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new EliminarConsultorioCommand { ConsultorioId = id });
            return NoContent();
        }
    }
}

using Core.Commands;
using Core.Dtos;
using Core.Queries;
using Domain.Seguridad;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Agenda de citas.
    ///
    /// Leer la agenda lo puede hacer cualquiera que haya iniciado sesión: el
    /// médico necesita ver su día igual que recepción. Agendar, mover y cancelar
    /// es trabajo de recepción y del administrador — un médico no se pone citas
    /// a sí mismo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CitasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CitasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Todas las citas.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CitaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodas()
        {
            var citas = await _mediator.Send(new ObtenerCitasQuery());
            return Ok(citas);
        }

        /// <summary>
        /// Citas por página. filter acepta una expresión dinámica (EstadoCitaID == 1)
        /// y orderBy un campo con sentido (FechaHoraInicio descending).
        /// </summary>
        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<CitaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1,
            int pageSize = 10,
            string? filter = null,
            string? orderBy = null)
        {
            var resultado = await _mediator.Send(new ObtenerCitasPaginadasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            });

            return Ok(resultado);
        }

        /// <summary>Una cita por su Id.</summary>
        [HttpGet("{id:long}", Name = "ObtenerCitaPorId")]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(long id)
        {
            var cita = await _mediator.Send(new ObtenerCitaPorIdQuery { CitaId = id });
            return Ok(cita);
        }

        /// <summary>Crea una cita. Recepción y administración.</summary>
        [HttpPost]
        [Authorize(Roles = RolesSistema.AdministradorORecepcion)]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Post([FromBody] CrearCitaDto cita)
        {
            var creada = await _mediator.Send(new CrearCitaCommand { Cita = cita });
            return CreatedAtAction(nameof(GetPorId), new { id = creada.CitaId }, creada);
        }

        /// <summary>Modifica una cita existente. Recepción y administración.</summary>
        [HttpPut("{id:long}")]
        [Authorize(Roles = RolesSistema.AdministradorORecepcion)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(long id, [FromBody] ActualizarCitaDto cita)
        {
            await _mediator.Send(new ActualizarCitaCommand { CitaId = id, Cita = cita });
            return NoContent();
        }

        /// <summary>Elimina una cita. Recepción y administración.</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = RolesSistema.AdministradorORecepcion)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            await _mediator.Send(new EliminarCitaCommand { CitaId = id });
            return NoContent();
        }
    }
}

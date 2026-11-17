using Core.Commands;
using Core.Dtos;
using Core.Queries;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
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

        /// <summary>Crea una cita.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CrearCitaDto cita)
        {
            var creada = await _mediator.Send(new CrearCitaCommand { Cita = cita });
            return CreatedAtAction(nameof(GetPorId), new { id = creada.CitaId }, creada);
        }

        /// <summary>Modifica una cita existente.</summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(long id, [FromBody] ActualizarCitaDto cita)
        {
            await _mediator.Send(new ActualizarCitaCommand { CitaId = id, Cita = cita });
            return NoContent();
        }

        /// <summary>Elimina una cita.</summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            await _mediator.Send(new EliminarCitaCommand { CitaId = id });
            return NoContent();
        }
    }
}

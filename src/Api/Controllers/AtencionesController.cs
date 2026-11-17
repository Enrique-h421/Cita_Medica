using Core.Dtos;
using Core.Features.Atenciones;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// La consulta médica. El ciclo es: se abre la atención, se documenta y se
    /// cierra. Al cerrarla, la cita que la originó pasa a "Atendida".
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AtencionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AtencionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AtencionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodas()
            => Ok(await _mediator.Send(new ObtenerAtencionesQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<AtencionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerAtencionesPaginadasQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(AtencionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(long id)
            => Ok(await _mediator.Send(new ObtenerAtencionPorIdQuery { AtencionId = id }));

        /// <summary>Historial clínico del paciente: todas sus atenciones, de la más reciente a la más vieja.</summary>
        [HttpGet("paciente/{pacienteId:long}")]
        [ProducesResponseType(typeof(List<AtencionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPorPaciente(long pacienteId)
            => Ok(await _mediator.Send(new ObtenerAtencionesPorPacienteQuery { PacienteId = pacienteId }));

        /// <summary>
        /// Abre la atención. Si el paciente no tiene expediente, se le abre uno
        /// automáticamente con su número correlativo.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AtencionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CrearAtencionDto atencion)
        {
            var creada = await _mediator.Send(new AbrirAtencionCommand { Atencion = atencion });
            return CreatedAtAction(nameof(GetPorId), new { id = creada.AtencionId }, creada);
        }

        /// <summary>
        /// Cierra la atención con los hallazgos del médico y deja la cita de
        /// origen en estado "Atendida".
        /// </summary>
        [HttpPut("{id:long}/cerrar")]
        [ProducesResponseType(typeof(AtencionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cerrar(long id, [FromBody] CerrarAtencionDto cierre)
            => Ok(await _mediator.Send(new CerrarAtencionCommand { AtencionId = id, Cierre = cierre }));

        /// <summary>Corrige lo documentado. Solo funciona mientras la atención siga abierta.</summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(long id, [FromBody] ActualizarAtencionDto atencion)
        {
            await _mediator.Send(new ActualizarAtencionCommand { AtencionId = id, Atencion = atencion });
            return NoContent();
        }

        /// <summary>
        /// Anula la atención. No la borra de la base: un registro clínico no se
        /// elimina, queda marcado como ANULADA.
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            await _mediator.Send(new AnularAtencionCommand { AtencionId = id });
            return NoContent();
        }
    }
}

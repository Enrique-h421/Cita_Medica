using Core.Dtos;
using Core.Features.Diagnosticos;
using Domain.Seguridad;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Los diagnósticos de una consulta concreta. Cuelgan de la atención porque
    /// no existen sin ella: son lo que el médico concluyó en esa consulta.
    ///
    /// Diagnosticar es acto médico, así que escribir solo lo pueden el médico y
    /// el administrador. Leer lo puede cualquiera con sesión, igual que el
    /// resto del historial.
    ///
    /// Solo se puede diagnosticar mientras la atención siga abierta: una vez
    /// cerrada, el expediente queda firmado.
    /// </summary>
    [ApiController]
    [Route("api/Atenciones/{atencionId:long}/diagnosticos")]
    [Produces("application/json")]
    public class AtencionDiagnosticosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AtencionDiagnosticosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Diagnósticos de la consulta: primero el principal, después los
        /// secundarios en el orden en que se agregaron.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AtencionDiagnosticoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodos(long atencionId)
            => Ok(await _mediator.Send(new ObtenerDiagnosticosDeAtencionQuery
            {
                AtencionId = atencionId
            }));

        /// <summary>
        /// Le agrega un diagnóstico del catálogo a la consulta. Si no se manda
        /// el tipo, el primero queda como principal y los siguientes como
        /// secundarios. Solo médico y administrador.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RolesSistema.AdministradorOMedico)]
        [ProducesResponseType(typeof(AtencionDiagnosticoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Post(long atencionId, [FromBody] AgregarDiagnosticoDto diagnostico)
        {
            var agregado = await _mediator.Send(new AgregarDiagnosticoCommand
            {
                AtencionId = atencionId,
                Datos = diagnostico
            });

            return CreatedAtAction(
                nameof(GetTodos),
                new { atencionId },
                agregado);
        }

        /// <summary>
        /// Quita un diagnóstico mal puesto de la consulta abierta. Solo médico y
        /// administrador.
        /// </summary>
        [HttpDelete("{atencionDiagnosticoId:long}")]
        [Authorize(Roles = RolesSistema.AdministradorOMedico)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(long atencionId, long atencionDiagnosticoId)
        {
            await _mediator.Send(new QuitarDiagnosticoCommand
            {
                AtencionId = atencionId,
                AtencionDiagnosticoId = atencionDiagnosticoId
            });

            return NoContent();
        }
    }
}

using Core.Dtos;
using Core.Features.EstadosCita;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Catálogo de estados de cita: Pendiente, Confirmada, Atendida, Cancelada
    /// y No asistió.
    ///
    /// Solo lectura, y lo puede consultar cualquiera que haya iniciado sesión:
    /// tanto quien agenda como quien atiende necesitan saber qué estados
    /// existen. No hay nada confidencial en una lista de etiquetas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EstadosCitaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EstadosCitaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EstadoCitaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerEstadosCitaQuery()));
    }
}

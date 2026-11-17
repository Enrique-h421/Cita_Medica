using Core.Dtos;
using Core.Features.Expedientes;
using GenericPersistence.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Expedientes clínicos. Solo lectura a propósito: el expediente se abre
    /// automáticamente en la primera atención del paciente y en una clínica no
    /// se elimina, así que no hay POST ni DELETE.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExpedientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExpedientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ExpedienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerExpedientesQuery()));

        [HttpGet("paginado")]
        [ProducesResponseType(typeof(PagedResult<ExpedienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginado(
            int pageNumber = 1, int pageSize = 10, string? filter = null, string? orderBy = null)
            => Ok(await _mediator.Send(new ObtenerExpedientesPaginadosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy
            }));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ExpedienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(long id)
            => Ok(await _mediator.Send(new ObtenerExpedientePorIdQuery { ExpedienteId = id }));

        /// <summary>El expediente de un paciente, buscado por su Id de paciente.</summary>
        [HttpGet("paciente/{pacienteId:long}")]
        [ProducesResponseType(typeof(ExpedienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorPaciente(long pacienteId)
            => Ok(await _mediator.Send(new ObtenerExpedientePorPacienteQuery { PacienteId = pacienteId }));
    }
}

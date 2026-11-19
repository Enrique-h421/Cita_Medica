using Catalogos.Core.Dtos;
using Catalogos.Core.Features.Usuarios;
using Catalogos.Domain.Seguridad;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers
{
    /// <summary>
    /// Usuarios y roles del sistema. Solo el Administrador: la lista de quién
    /// tiene acceso a la clínica no es información para recepción ni para los
    /// médicos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = RolesSistema.Administrador)]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTodos()
            => Ok(await _mediator.Send(new ObtenerUsuariosQuery()));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPorId(int id)
            => Ok(await _mediator.Send(new ObtenerUsuarioPorIdQuery { UsuarioId = id }));

        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
            => Ok(await _mediator.Send(new ObtenerRolesQuery()));
    }
}

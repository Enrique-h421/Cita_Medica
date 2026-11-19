using Catalogos.Core.Dtos;
using Catalogos.Core.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers
{
    /// <summary>
    /// Autenticación. Este es el único lugar del sistema que se puede llamar sin
    /// token: si hiciera falta un token para pedir un token, nadie podría entrar.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Inicia sesión. Devuelve el token y los datos de la sesión.
        /// Credenciales de la demo: admin / Admin123!
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SesionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto credenciales)
            => Ok(await _mediator.Send(new LoginCommand { Credenciales = credenciales }));

        /// <summary>
        /// Quién soy, según el token que vengo presentando. No toca la base de
        /// datos: todo esto sale del propio token. Sirve para comprobar que la
        /// autenticación quedó bien armada.
        /// </summary>
        [HttpGet("yo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Yo()
        {
            return Ok(new
            {
                usuarioId = User.FindFirst("sub")?.Value,
                nombreUsuario = User.FindFirst("nombreUsuario")?.Value,
                nombreCompleto = User.FindFirst("nombreCompleto")?.Value,
                rol = User.FindFirst("role")?.Value,
                rolId = User.FindFirst("rolId")?.Value,
                medicoId = User.FindFirst("medicoId")?.Value,
                sucursalId = User.FindFirst("sucursalId")?.Value,
                autenticado = User.Identity?.IsAuthenticated ?? false
            });
        }
    }
}

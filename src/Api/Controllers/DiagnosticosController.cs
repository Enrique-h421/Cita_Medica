using Core.Dtos;
using Core.Features.Diagnosticos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Catálogo de diagnósticos codificados con CIE-10, la clasificación de
    /// enfermedades de la OMS.
    ///
    /// Solo lectura, y lo puede consultar cualquiera que haya iniciado sesión:
    /// es una lista de etiquetas estándar, no tiene nada de ningún paciente.
    /// El catálogo lo siembra la clínica por SQL (sql/05_Diagnosticos.sql).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DiagnosticosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DiagnosticosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Busca diagnósticos por nombre o por código. Sin búsqueda devuelve los
        /// primeros del catálogo en orden alfabético, que es lo que necesita un
        /// buscador antes de que el usuario escriba algo.
        /// </summary>
        /// <param name="busqueda">Texto a buscar en el nombre o en el código, por ejemplo "faring" o "J02".</param>
        /// <param name="tope">Cuántos resultados devolver. Por defecto 20, máximo 100.</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<DiagnosticoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodos(string? busqueda = null, int tope = 20)
            => Ok(await _mediator.Send(new BuscarDiagnosticosQuery
            {
                Busqueda = busqueda,
                Tope = tope
            }));
    }
}

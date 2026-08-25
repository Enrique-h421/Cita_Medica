using Core;
using Core.Queries;
using Core.Commands;
using Domain.AgendaYAtencion;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly IMediator _mediator;

      
        public CitasController(IMediator mediator)
        {
            _mediator = mediator;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetTodas()
        {
            var citas = await _mediator.Send(new ObtenerCitasQuery());
            return Ok(citas);
        }

        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cita cita)
        {
            var resultado = await _mediator.Send(new CrearCitaCommand { NuevaCita = cita });
            return Ok(resultado);
        }

       
    }
}
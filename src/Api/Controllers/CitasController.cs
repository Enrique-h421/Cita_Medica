using Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ICitaRepository _citaRepository;

        public CitasController(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodas()
        {
            var citas = await _citaRepository.ObtenerTodasLasCitasAsync();
            return Ok(citas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPorId(long id)
        {
            var cita = await _citaRepository.ObtenerCitaPorIdAsync(id);
            if (cita == null)
                return NotFound($"No se encontró la cita con ID {id}");

            return Ok(cita);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Core.GenericRepository;
using Domain.AgendaYAtencion;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenericoController : ControllerBase
    {
        private readonly IGenericRepository<Cita> _repository;

        public GenericoController(IGenericRepository<Cita> repository)
        {
            _repository = repository;
        }

        // GET: api/generico
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var citas = await _repository.ObtenerTodosAsync();
            return Ok(citas);
        }

        // GET: api/generico/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var cita = await _repository.ObtenerPorIdAsync(id);
            if (cita == null) return NotFound();
            return Ok(cita);
        }

        // POST: api/generico
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cita cita)
        {
            await _repository.CrearAsync(cita);
            return Ok(cita);
        }

        // PUT: api/generico/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] Cita cita)
        {
            var existente = await _repository.ObtenerPorIdAsync(id);
            if (existente == null) return NotFound();

            await _repository.ActualizarAsync(cita);
            return Ok(cita);
        }

        // DELETE: api/generico/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existente = await _repository.ObtenerPorIdAsync(id);
            if (existente == null) return NotFound();

            await _repository.EliminarAsync(id);
            return Ok();
        }
    }
}
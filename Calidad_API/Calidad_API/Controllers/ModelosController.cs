using Calidad_API.DTOs.Modelos;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ModelosController : ControllerBase
    {
        private readonly IModeloService _service;
        public ModelosController(IModeloService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModeloDto>>> GetAll([FromQuery] bool soloActivos = true)
            => Ok(await _service.GetAllAsync(soloActivos));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ModeloDto>> GetById(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<ModeloDto>> GetByCodigo(string codigo)
        {
            var item = await _service.GetByCodigoAsync(codigo);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<ModeloDto>> Create([FromBody] ModeloCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdModelo }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<ModeloDto>> Update(long id, [FromBody] ModeloUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
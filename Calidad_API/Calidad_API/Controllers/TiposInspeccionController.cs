using Calidad_API.DTOs.TiposInspeccion;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TiposInspeccionController : ControllerBase
    {
        private readonly ITipoInspeccionService _service;
        public TiposInspeccionController(ITipoInspeccionService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoInspeccionDto>>> GetAll([FromQuery] bool soloActivos = true)
            => Ok(await _service.GetAllAsync(soloActivos));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoInspeccionDto>> GetById(short id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TipoInspeccionDto>> Create([FromBody] TipoInspeccionCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdTipoInspeccion }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TipoInspeccionDto>> Update(short id, [FromBody] TipoInspeccionUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(short id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
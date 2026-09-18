using Calidad_API.DTOs.UnidadesNegocio;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UnidadesNegocioController : ControllerBase
    {
        private readonly IUnidadNegocioService _service;
        public UnidadesNegocioController(IUnidadNegocioService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadNegocioDto>>> GetAll([FromQuery] bool soloActivas = true)
            => Ok(await _service.GetAllAsync(soloActivas));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<UnidadNegocioDto>> GetById(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UnidadNegocioDto>> Create([FromBody] UnidadNegocioCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUnidadNegocio }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UnidadNegocioDto>> Update(long id, [FromBody] UnidadNegocioUpdateDto dto)
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
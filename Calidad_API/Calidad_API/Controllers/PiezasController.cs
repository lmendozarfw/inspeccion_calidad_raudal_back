using Calidad_API.DTOs.Piezas;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PiezasController : ControllerBase
    {
        private readonly IPiezaService _service;
        public PiezasController(IPiezaService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PiezaDto>>> GetAll([FromQuery] bool soloActivas = true)
            => Ok(await _service.GetAllAsync(soloActivas));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<PiezaDto>> GetById(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<PiezaDto>> Create([FromBody] PiezaCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdPieza }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<PiezaDto>> Update(long id, [FromBody] PiezaUpdateDto dto)
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
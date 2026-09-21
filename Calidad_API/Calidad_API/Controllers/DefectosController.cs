using Calidad_API.DTOs.Defectos;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DefectosController : ControllerBase
    {
        private readonly IDefectoService _defectoService;

        public DefectosController(IDefectoService defectoService)
        {
            _defectoService = defectoService;
        }

        /// <summary>Lista defectos de un área (el más usado en captura)</summary>
        [HttpGet("operacion/{idOperacion:long}")]
        public async Task<ActionResult<IEnumerable<DefectoDto>>> GetByArea(long idOperacion, [FromQuery] bool soloActivos = true)
        {
            var result = await _defectoService.GetByAreaAsync(idOperacion, soloActivos);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<DefectoDto>> GetById(long id)
        {
            var defecto = await _defectoService.GetByIdAsync(id);
            return defecto is null ? NotFound(new { mensaje = "Defecto no encontrado." }) : Ok(defecto);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<DefectoDto>> Create([FromBody] DefectoCreateDto dto)
        {
            try
            {
                var created = await _defectoService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.IdDefecto }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<DefectoDto>> Update(long id, [FromBody] DefectoUpdateDto dto)
        {
            var updated = await _defectoService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _defectoService.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}

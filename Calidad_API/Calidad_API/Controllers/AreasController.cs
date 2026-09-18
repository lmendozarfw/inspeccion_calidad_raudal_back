using Calidad_API.DTOs.Operaciones;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AreasController : ControllerBase
    {
        private readonly IOperacionService _areaService;

        public AreasController(IOperacionService areaService)
        {
            _areaService = areaService;
        }
            
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetAll([FromQuery] bool soloActivas = true)
        {
            var result = await _areaService.GetAllAsync(soloActivas);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<OperacionDto>> GetById(long id)
        {
            var area = await _areaService.GetByIdAsync(id);
            return area is null ? NotFound() : Ok(area);
        }

        [HttpGet("proceso/{proceso}")]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetByProceso(string proceso)
        {
            var result = await _areaService.GetByProcesoAsync(proceso);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<OperacionDto>> Create([FromBody] DTOs.Operaciones.OperacionCreateDto dto)
        {
            var created = await _areaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdOperacion }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<OperacionDto>> Update(long id, [FromBody] OperacionUpdateDto dto)
        {
            var updated = await _areaService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _areaService.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}

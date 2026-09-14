using Calidad_API.DTOs.Area;
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
        private readonly IAreaService _areaService;

        public AreasController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AreaDto>>> GetAll([FromQuery] bool soloActivas = true)
        {
            var result = await _areaService.GetAllAsync(soloActivas);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<AreaDto>> GetById(long id)
        {
            var area = await _areaService.GetByIdAsync(id);
            return area is null ? NotFound() : Ok(area);
        }

        [HttpGet("proceso/{proceso}")]
        public async Task<ActionResult<IEnumerable<AreaDto>>> GetByProceso(string proceso)
        {
            var result = await _areaService.GetByProcesoAsync(proceso);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<AreaDto>> Create([FromBody] AreaCreateDto dto)
        {
            var created = await _areaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdArea }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<AreaDto>> Update(long id, [FromBody] AreaUpdateDto dto)
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

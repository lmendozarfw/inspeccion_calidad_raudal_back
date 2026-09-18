using Calidad_API.DTOs.Departamentos;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoService _service;
        public DepartamentosController(IDepartamentoService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetAll([FromQuery] bool soloActivos = true)
            => Ok(await _service.GetAllAsync(soloActivos));

        [HttpGet("unidad-negocio/{idUnidadNegocio:long}")]
        public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetByUnidad(long idUnidadNegocio)
            => Ok(await _service.GetByUnidadNegocioAsync(idUnidadNegocio));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<DepartamentoDto>> GetById(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<DepartamentoDto>> Create([FromBody] DepartamentoCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdDepartamento }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<DepartamentoDto>> Update(long id, [FromBody] DepartamentoUpdateDto dto)
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
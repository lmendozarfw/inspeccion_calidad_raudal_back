using Calidad_API.DTOs.Operaciones;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OperacionesController : ControllerBase
    {
        private readonly IOperacionService _areaService;

        private long GetUserId() =>
             long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public OperacionesController(IOperacionService areaService)
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
            return area is null ? NotFound(new { mensaje = "Área no encontrada." }) : Ok(area);
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
            return updated is null ? NotFound(new { mensaje = "Área no encontrada." }) : Ok(updated);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _areaService.DeleteAsync(id);
            return ok ? NoContent() : NotFound(new { mensaje = "Área no encontrada." });
        }

        /// <summary>
        /// Operaciones donde el usuario logueado tiene permisos (para captura en el front).
        /// </summary>
        [HttpGet("mis-permisos")]
        public async Task<ActionResult<IEnumerable<OperacionPermisoDto>>> MisPermisos()
        {
            var result = await _areaService.GetMisPermisosAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("departamento/{idDepartamento:long}")]
        public async Task<ActionResult<IEnumerable<OperacionDto>>> GetByDepartamento(long idDepartamento)
        {
            var result = await _areaService.GetByDepartamentoAsync(idDepartamento);
            return Ok(result);
        }
    }
}

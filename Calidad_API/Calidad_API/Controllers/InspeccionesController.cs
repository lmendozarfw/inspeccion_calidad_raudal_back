using System.Security.Claims;
using Calidad_API.DTOs.Inspecciones;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class InspeccionesController : ControllerBase
    {
        private readonly IInspeccionService _inspeccionService;

        public InspeccionesController(IInspeccionService inspeccionService)
        {
            _inspeccionService = inspeccionService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.Parse(claim!);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<InspeccionDto>> GetById(long id)
        {
            var i = await _inspeccionService.GetByIdAsync(id);
            return i is null ? NotFound() : Ok(i);
        }

        [HttpGet("transfer/{idTransfer:long}")]
        public async Task<ActionResult<IEnumerable<InspeccionDto>>> GetByTransfer(long idTransfer)
        {
            var list = await _inspeccionService.GetByTransferAsync(idTransfer);
            return Ok(list);
        }

        [HttpGet("area/{idArea:long}")]
        public async Task<ActionResult<IEnumerable<InspeccionDto>>> GetByArea(
            long idArea,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var list = await _inspeccionService.GetByAreaAsync(idArea, desde, hasta);
            return Ok(list);
        }

        /// <summary>Abre una nueva inspección sobre un transfer + área</summary>
        [HttpPost]
        public async Task<ActionResult<InspeccionDto>> Crear([FromBody] InspeccionCreateDto dto)
        {
            try
            {
                var created = await _inspeccionService.CrearAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = created.IdInspeccion }, created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(); // o Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Agrega un defecto (piocha / reproceso) a la inspección</summary>
        [HttpPost("{idInspeccion:long}/detalles")]
        public async Task<ActionResult<InspeccionDetalleDto>> AgregarDetalle(
            long idInspeccion,
            [FromBody] InspeccionDetalleCreateDto dto)
        {
            try
            {
                var detalle = await _inspeccionService.AgregarDetalleAsync(idInspeccion, dto, GetUserId());
                return Ok(detalle);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Cierra la inspección</summary>
        [HttpPut("{id:long}/cerrar")]
        public async Task<ActionResult<InspeccionDto>> Cerrar(long id, [FromBody] InspeccionCerrarDto dto)
        {
            try
            {
                var result = await _inspeccionService.CerrarAsync(id, dto);
                return result is null ? NotFound() : Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("detalles/{idDetalle:long}")]
        public async Task<IActionResult> EliminarDetalle(long idDetalle)
        {
            try
            {
                var ok = await _inspeccionService.EliminarDetalleAsync(idDetalle);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
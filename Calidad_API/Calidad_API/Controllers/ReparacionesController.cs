using System.Security.Claims;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ReparacionesController : ControllerBase
    {
        private readonly IReparacionService _reparacionService;

        public ReparacionesController(IReparacionService reparacionService)
        {
            _reparacionService = reparacionService;
        }

        private long GetUserId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("detalle/{idDetalle}")]
        public async Task<ActionResult<List<ReparacionDto>>> GetByDetalle(long idDetalle)
        {
            var reparaciones = await _reparacionService.GetByAllAsync(idDetalle);
            return Ok(reparaciones);
        }

        [HttpPost]
        public async Task<ActionResult<ReparacionDto>> Crear([FromBody] ReparacionCreateDto dto)
        {
            try
            {
                var created = await _reparacionService.CreateAsync(dto, GetUserId());
                return created is null ? NotFound() : Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

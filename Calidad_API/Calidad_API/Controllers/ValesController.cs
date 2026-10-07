using System.Security.Claims;
using Calidad_API.DTOs.Vales;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ValesController : ControllerBase
    {
        private readonly IValeService _valeService;

        public ValesController(IValeService valeService)
        {
            _valeService = valeService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.Parse(claim!);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ValeDto>> GetById(long id)
        {
            var vale = await _valeService.GetByIdAsync(id);
            return vale is null ? NotFound() : Ok(vale);
        }

        [HttpGet("folio/{folio}")]
        public async Task<ActionResult<ValeDto>> GetByFolio(string folio)
        {
            var vale = await _valeService.GetByFolioAsync(folio);
            return vale is null ? NotFound() : Ok(vale);
        }

        [HttpGet("detalle/{idDetalle:long}")]
        public async Task<ActionResult<IEnumerable<ValeDto>>> GetByDetalle(long idDetalle)
        {
            var list = await _valeService.GetByInspeccionDetalleAsync(idDetalle);
            return Ok(list);
        }

        /// <summary>
        /// Genera un vale de material a partir de un detalle de inspección.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ValeDto>> Generar([FromBody] ValeCreateDto dto)
        {
            try
            {
                var vale = await _valeService.GenerarAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = vale.IdVale }, vale);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}/estado")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<ValeDto>> ActualizarEstado(long id, [FromBody] ValeUpdateEstadoDto dto)
        {
            try
            {
                var vale = await _valeService.ActualizarEstadoAsync(id, dto);
                return vale is null ? NotFound() : Ok(vale);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:long}/pdf")]
        public async Task<IActionResult> DescargarPdf(long id)
        {
            var vale = await _valeService.GetByIdAsync(id);
            if (vale is null) return NotFound();

            if (string.IsNullOrWhiteSpace(vale.RutaPdf) || !System.IO.File.Exists(vale.RutaPdf))
                return NotFound(new { message = "El PDF del vale aún no está disponible." });

            var bytes = await System.IO.File.ReadAllBytesAsync(vale.RutaPdf);
            return File(bytes, "application/pdf", $"{vale.Folio}.pdf");
        }

        [HttpGet("inspeccion/{idInspeccion:long}")]
        public async Task<ActionResult<IEnumerable<ValeDto>>> GetByInspeccion(long idInspeccion)
    => Ok(await _valeService.GetByInspeccionAsync(idInspeccion));

        [HttpPost("corte")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<CorteEjecutarResponse>> EjecutarCorte(
    [FromServices] ICorteValeService corteService)
        {
            var result = await corteService.EjecutarCorteAsync();
            return Ok(result);
        }

        [HttpGet("cortes/pendientes")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<IEnumerable<CorteResumenDto>>> CortesPendientes(
            [FromServices] ICorteValeService corteService)
            => Ok(await corteService.GetPendientesAsync());

        [HttpGet("cortes/{idCorte:long}")]
        [Authorize]
        public async Task<ActionResult<CorteDetalleDto>> GetCorte(
            long idCorte,
            [FromServices] ICorteValeService corteService)
        {
            var c = await corteService.GetByIdAsync(idCorte);
            return c is null ? NotFound() : Ok(c);
        }


        [HttpPost("cortes/{idCorte:long}/autorizar")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<ValeDto>> AutorizarCorte(
    long idCorte,
    [FromBody] CorteAutorizarDto dto,
    [FromServices] ICorteValeService corteService)
        {
            try
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var idUsuario = long.Parse(claim!);
                var vale = await corteService.AutorizarCorteAsync(idCorte, dto.CodigoAutorizacion, idUsuario);
                return Ok(vale);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
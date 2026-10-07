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

        private static (DateTime desde, DateTime hasta) Rango(DateTime? desde, DateTime? hasta)
        {
            var h = hasta?.Date ?? DateTime.UtcNow.Date;
            var d = desde?.Date ?? h.AddDays(-7);
            return (d, h);
        }

        public InspeccionesController(IInspeccionService inspeccionService)
        {
            _inspeccionService = inspeccionService;
        }

        private long GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.Parse(claim!);
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<InspeccionDto>>> GetAll()
        {
            var resultado = await _inspeccionService.GetAll();
            return Ok(resultado);
        }

        [HttpGet("reporte")]
        public async Task<ActionResult<IEnumerable<InspeccionReporteDto>>> GetReport([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = Rango(desde, hasta);
            var response = await _inspeccionService.GetReport(d, h);
            return Ok(response);
        }

        [HttpGet("por-operacion/reporte")]
        public async Task<ActionResult<IEnumerable<InspeccionPorOperacionReporteDto>>> GetReportPorOperacion([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = Rango(desde, hasta);
            var response = await _inspeccionService.GetReportePorOpracion(d, h);
            return Ok(response);
        }

        [HttpGet("indicadores/reporte")]
        public async Task<ActionResult<IEnumerable<InspeccionReporteDto>>> GetReportIndicadores([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = Rango(desde, hasta);
            var response = await _inspeccionService.GetReportIndicadores(d, h);
            return Ok(response);
        }

        [HttpGet("por-lotes/reporte")]
        public async Task<ActionResult<IEnumerable<InspeccionReporteDto>>> GetReportByLotes([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = Rango(desde, hasta);
            var response = await _inspeccionService.GetReportByLotes(d, h);
            return Ok(response);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<InspeccionDto>>> Buscar(
            [FromQuery] string? search)
        {
            return Ok(await _inspeccionService.Search(search));
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

        [HttpGet("por-transfer")]
        public async Task<ActionResult<List<InspeccionDto>>> GetAbiertasPorTransfer(
            [FromQuery] string programa,
            [FromQuery] string lote,
            [FromQuery] string modelo)
        {
            if (string.IsNullOrWhiteSpace(programa) || string.IsNullOrWhiteSpace(lote) || string.IsNullOrWhiteSpace(modelo))
                return BadRequest(new { message = "Programa, lote y modelo son obligatorios." });

            var list = await _inspeccionService.GetAbiertasByTransferAsync(programa, lote, modelo);
            return Ok(list);
        }

        [HttpGet("operacion/{idOperacion:long}")]
        public async Task<ActionResult<IEnumerable<InspeccionDto>>> GetByArea(
            long idOperacion,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var list = await _inspeccionService.GetByAreaAsync(idOperacion, desde, hasta);
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


        /// <summary>
        /// Registro unificado: transfer (programa+lote) + inspecciones + defectos en un solo POST.
        /// Modelo y lista son opcionales.
        /// </summary>
        [HttpPost("registrar")]
        public async Task<ActionResult<InspectionRegisterResponse>> Registrar(
            [FromBody] InspectionRegisterRequest request)
        {
            try
            {
                Console.WriteLine("=================OBTENIENDO PETICION=====================");
                var result = await _inspeccionService.RegistrarAsync(request, GetUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
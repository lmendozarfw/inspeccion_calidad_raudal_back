using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reportes;

        public ReportesController(IReporteService reportes)
        {
            _reportes = reportes;
        }

        private static (DateTime desde, DateTime hasta) Rango(DateTime? desde, DateTime? hasta)
        {
            var h = hasta?.Date ?? DateTime.UtcNow.Date;
            var d = desde?.Date ?? h.AddDays(-7);
            return (d, h);
        }

        private static FileContentResult Excel(byte[] bytes, string nombre)
            => new(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = nombre };

        [HttpGet("defectos-detalle")]
        public async Task<IActionResult> DefectosDetalle(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] long? idOperacion, [FromQuery] string? programa,
            [FromQuery] string? lote, [FromQuery] string? tipoRegistro)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.DefectosDetalleExcelAsync(d, h, idOperacion, programa, lote, tipoRegistro);
            return Excel(bytes, $"defectos_detalle_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("defectos-resumen")]
        public async Task<IActionResult> DefectosResumen(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] long? idOperacion, [FromQuery] string? programa)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.DefectosResumenExcelAsync(d, h, idOperacion, programa);
            return Excel(bytes, $"defectos_resumen_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("por-lote")]
        public async Task<IActionResult> PorLote(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] string? programa, [FromQuery] string? lote)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.PorLoteExcelAsync(d, h, programa, lote);
            return Excel(bytes, $"reporte_lote_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("por-operacion")]
        public async Task<IActionResult> PorOperacion(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.PorOperacionAsync(d, h);
            return Excel(bytes, $"reporte_lote_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("vales")]
        public async Task<IActionResult> Vales(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] string? estado, [FromQuery] long? idOperacion)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.ValesExcelAsync(d, h, estado, idOperacion);
            return Excel(bytes, $"vales_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("indicadores")]
        public async Task<IActionResult> Indicadores(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] long? idOperacion, [FromQuery] string? programa)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.IndicadoresExcelAsync(d, h, idOperacion, programa);
            return Excel(bytes, $"indicadores_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }

        [HttpGet("inspecciones")]
        public async Task<IActionResult> Inspecciones(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] long? idOperacion, [FromQuery] string? lote)
        {
            var (d, h) = Rango(desde, hasta);
            var bytes = await _reportes.InspeccionesExcelAsync(d, h, idOperacion, lote);
            return Excel(bytes, $"inspecciones_{d:yyyyMMdd}_{h:yyyyMMdd}.xlsx");
        }
    }
}
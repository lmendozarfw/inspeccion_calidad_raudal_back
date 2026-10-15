using Calidad_API.DTOs.Dashboard;
using Calidad_API.Interfaces;
using Calidad_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dhasboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dhasboardService = dashboardService;
    }

    [HttpGet()]
    public async Task<ActionResult<DashboardDto>> GetAll([FromQuery] DashboardDateRange range)
    {
        var response = await _dhasboardService.GetAll(range);
        return Ok(response);
    }

    [HttpGet("detalle")]
    public async Task<ActionResult<IReadOnlyList<DashboardDetalleItemDto>>> GetDetalle(
    [FromQuery] DashboardDetalleFiltro filtro)
    {
        var list = await _dhasboardService.GetDetalleAsync(filtro);
        return Ok(list);
    }

    [HttpGet("heatmap")]
    public async Task<ActionResult<HeatmapOperacionDto>> Heatmap([FromQuery] DateTime? fecha)
    => Ok(await _dhasboardService.GetHeatmapPorOperacionAsync(fecha));

    [HttpGet("top-por-operacion")]
    public async Task<ActionResult<List<TopDefectosPorOperacionDto>>> TopPorOperacion([FromQuery] DateTime? fecha)
        => Ok(await _dhasboardService.GetTopPorOperacionDiaAsync(fecha));

    [HttpGet("bloque-rango")]
    public async Task<ActionResult<DashboardSemanaBloqueDto>> BloqueRango(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(await _dhasboardService.GetBloqueRangoAsync(desde, hasta));


}
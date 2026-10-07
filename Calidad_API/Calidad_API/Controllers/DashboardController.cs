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


}
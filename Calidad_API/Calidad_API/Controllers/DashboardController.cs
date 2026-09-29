using Calidad_API.DTOs.Dashboard;
using Calidad_API.Interfaces;
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
}
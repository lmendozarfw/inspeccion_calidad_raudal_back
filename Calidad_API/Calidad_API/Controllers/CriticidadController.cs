using Calidad_API.DTOs.Criticidad;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class CriticidadController : ControllerBase
{
    private readonly ICriticidadService _criticidadService;

    public CriticidadController(ICriticidadService criticidadService)
    {
        _criticidadService = criticidadService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CriticidadDto>>> GetAll()
    {
        var result = await _criticidadService.GetAllAsync();
        return Ok(result);
    }
}
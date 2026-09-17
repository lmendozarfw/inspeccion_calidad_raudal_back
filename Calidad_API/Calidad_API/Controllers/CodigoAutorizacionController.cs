using System.Security.Claims;
using Calidad_API.DTOs.CodigoAutorizacion;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers;


[Route("[controller]")]
[ApiController]
[Authorize]
public class CodigoAutorizacionController : ControllerBase
{
    
    private readonly ICodigoAutorizacionService _codigoAutorizacionService;

    public CodigoAutorizacionController(ICodigoAutorizacionService codigoAutorizacionService)
    {
        _codigoAutorizacionService = codigoAutorizacionService;
    }
    
    [HttpGet]
    public async Task<ActionResult<CodigoAutorizacionDto>> ObtenerCodigoAutorizacion()
    {
        var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine("id usuario" + " " + idUsuarioStr);
        if (!long.TryParse(idUsuarioStr, out var idUsuario))
        {
            return Unauthorized();
        }
        var codigo = await _codigoAutorizacionService.ObtenerCodigoAutorizacionAsync(idUsuario);
        return Ok(codigo);
    }
}
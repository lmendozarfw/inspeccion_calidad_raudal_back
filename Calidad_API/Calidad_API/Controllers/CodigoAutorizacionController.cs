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

    [HttpGet("ObtenerCodigosPorUsuario/{idUsuario}")]
    public async Task<ActionResult<IEnumerable<CodigoAutorizacionDto>>> ObtenerCodigosPorUsuarioAsync(long idUsuario)
    {
        var codigos = await _codigoAutorizacionService.GetCodigosAutorizacionByUsuarioAsync(idUsuario);
        return Ok(codigos);
    }
    
    [HttpGet("ObtenerCodigoNuevo")]
    public async Task<ActionResult<CodigoAutorizacionDto>> ObtenerCodigoAutorizacion()
    {
var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(idUsuarioStr, out var idUsuario))
            {
                return Unauthorized(new { mensaje = "Token inválido o usuario no autenticado." });
            }
        var codigo = await _codigoAutorizacionService.ObtenerCodigoAutorizacionAsync(idUsuario);
        return Ok(codigo);
    }
}
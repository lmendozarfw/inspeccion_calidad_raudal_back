using System.Security.Claims;
using Calidad_API.DTOs;
using Calidad_API.DTOs.Usuario;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
        
        [HttpGet("Me")]
        [Authorize]
        public async Task<ActionResult<UsuarioMeDto>> GetMe()
        {
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(idUsuarioStr, out var idUsuario))
            {
                return Unauthorized(new { mensaje = "Token inválido o usuario no autenticado." });
            }

            var usuario = await _authService.ObtenerUsuarioActualAsync(idUsuario);

            if (usuario is null)
            {
                return Unauthorized(new { mensaje = "Token inválido o usuario no autenticado." });
            }

            return Ok(usuario);
        }
    }
}

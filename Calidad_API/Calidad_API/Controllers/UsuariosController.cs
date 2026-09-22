using Calidad_API.DTOs.Usuario;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUserRepository _repository;
        public UsuariosController(IUserRepository repository) => _repository = repository;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll(string? search)
            => Ok(await _repository.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetById(long id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto)
        {
            var created = await _repository.CreateAsync(dto);
            return created is null
                ? BadRequest(new { mensaje = "Datos inválidos, nombre de usuario ya existente o password no válido." })
                : CreatedAtAction(nameof(GetById), new { id = created.IdUsuario }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> Update(long id, [FromBody] UpdateUsuarioDto dto)
        {
            var updated = await _repository.UpdateAsync(id, dto);
            return updated is null ? NotFound(new { mensaje = "Usuario no encontrado o datos inválidos." }) : Ok(updated);
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> ToggleActivo(long id, [FromBody] ToggleActivoDto dto)
        {
            var usuario = await _repository.ToggleAsync(id, dto);
            return usuario is null ? NotFound(new { mensaje = "Usuario no encontrado." }) : Ok(usuario);
        }

        [HttpPut("{id}/operaciones")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> AsignarOperaciones(long id, [FromBody] AsignarOperacionesDto dto)
        {
            var usuario = await _repository.AsignarOperacionesAsync(id, dto);
            return usuario is null
                ? BadRequest(new { mensaje = "Datos inválidos, operaciones no encontradas o usuario no encontrado." })
                : Ok(usuario);
        }
        
        [HttpPut("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPassword(long id, [FromBody] CambiarPasswordDto dto)
        {
            // Solo el propio usuario o un administrador debería poder cambiar la contraseña
            // (puedes reforzar esto después con roles)
            var resultado = await _repository.CambiarPasswordAsync(id, dto);

            if (!resultado)
                return BadRequest(new { mensaje = "No se pudo cambiar la contraseña. Verifique la contraseña actual." });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }
}
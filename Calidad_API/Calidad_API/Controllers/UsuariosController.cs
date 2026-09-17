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
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
            => Ok(await _repository.GetAllAsync());

        [HttpGet("{id:long}")]
        public async Task<ActionResult<UsuarioDto>> GetById(long id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario is null ? NotFound() : Ok(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto)
        {
            var created = await _repository.CreateAsync(dto);
            return created is null ? BadRequest() : CreatedAtAction(nameof(GetById), new { id = created.IdUsuario }, created);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> Update(long id, [FromBody] UpdateUsuarioDto dto)
        {
            var updated = await _repository.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpPut("{id:long}/activo")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> ToggleActivo(long id, [FromBody] ToggleActivoDto dto)
        {
            var usuario = await _repository.ToggleAsync(id, dto);
            return usuario is null ? NotFound() : Ok(usuario);
        }

        [HttpPut("{id:long}/operaciones")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<UsuarioDto>> AsignarOperaciones(long id, [FromBody] AsignarOperacionesDto dto)
        {
            var usuario = await _repository.AsignarOperacionesAsync(id, dto);
            return usuario is null ? BadRequest() : Ok(usuario);
        }
    }
}
using Calidad_API.DTOs.Roles;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _service;
        public RolesController(IRolService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<RolDto>> GetById(short id)
        {
            var rol = await _service.GetByIdAsync(id);
            return rol is null ? NotFound() : Ok(rol);
        }
    }
}
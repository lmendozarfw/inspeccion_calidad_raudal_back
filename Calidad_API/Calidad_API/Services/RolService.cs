using Calidad_API.Data;
using Calidad_API.DTOs.Roles;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class RolService : IRolService
    {
        private readonly ApplicationDbContext _context;
        public RolService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<RolDto>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking()
                .OrderBy(r => r.Codigo)
                .Select(r => new RolDto(r.IdRol, r.Codigo, r.Nombre))
                .ToListAsync();
        }

        public async Task<RolDto?> GetByIdAsync(short id)
        {
            var rol = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.IdRol == id);
            return rol is null ? null : new RolDto(rol.IdRol, rol.Codigo, rol.Nombre);
        }
    }
}
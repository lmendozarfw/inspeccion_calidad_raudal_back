using Calidad_API.Data;
using Calidad_API.DTOs.UnidadesNegocio;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class UnidadNegocioService : IUnidadNegocioService
    {
        private readonly ApplicationDbContext _context;
        public UnidadNegocioService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<UnidadNegocioDto>> GetAllAsync(bool soloActivas = true)
        {
            var q = _context.UnidadesNegocio.AsNoTracking();
            if (soloActivas) q = q.Where(x => x.Activo);
            return await q.OrderBy(x => x.Codigo)
                .Select(x => new UnidadNegocioDto(x.IdUnidadNegocio, x.Codigo, x.Nombre, x.Activo, x.FechaAlta))
                .ToListAsync();
        }

        public async Task<UnidadNegocioDto?> GetByIdAsync(long id)
        {
            var x = await _context.UnidadesNegocio.AsNoTracking().FirstOrDefaultAsync(u => u.IdUnidadNegocio == id);
            return x is null ? null : new UnidadNegocioDto(x.IdUnidadNegocio, x.Codigo, x.Nombre, x.Activo, x.FechaAlta);
        }

        public async Task<UnidadNegocioDto> CreateAsync(UnidadNegocioCreateDto dto)
        {
            var e = new UnidadNegocio
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };
            _context.UnidadesNegocio.Add(e);
            await _context.SaveChangesAsync();
            return new UnidadNegocioDto(e.IdUnidadNegocio, e.Codigo, e.Nombre, e.Activo, e.FechaAlta);
        }

        public async Task<UnidadNegocioDto?> UpdateAsync(long id, UnidadNegocioUpdateDto dto)
        {
            var e = await _context.UnidadesNegocio.FindAsync(id);
            if (e is null) return null;
            e.Nombre = dto.Nombre.Trim();
            e.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return new UnidadNegocioDto(e.IdUnidadNegocio, e.Codigo, e.Nombre, e.Activo, e.FechaAlta);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var e = await _context.UnidadesNegocio.FindAsync(id);
            if (e is null) return false;
            e.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
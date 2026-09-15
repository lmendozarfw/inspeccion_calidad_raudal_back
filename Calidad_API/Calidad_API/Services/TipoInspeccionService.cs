using Calidad_API.Data;
using Calidad_API.DTOs.TiposInspeccion;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class TipoInspeccionService : ITipoInspeccionService
    {
        private readonly ApplicationDbContext _context;
        public TipoInspeccionService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<TipoInspeccionDto>> GetAllAsync(bool soloActivos = true)
        {
            var q = _context.TiposInspeccion.AsNoTracking().AsQueryable();
            if (soloActivos) q = q.Where(x => x.Activo);
            return await q.OrderBy(x => x.Codigo)
                .Select(x => new TipoInspeccionDto(x.IdTipoInspeccion, x.Codigo, x.Nombre, x.Activo))
                .ToListAsync();
        }

        public async Task<TipoInspeccionDto?> GetByIdAsync(short id)
        {
            var x = await _context.TiposInspeccion.AsNoTracking().FirstOrDefaultAsync(t => t.IdTipoInspeccion == id);
            return x is null ? null : new TipoInspeccionDto(x.IdTipoInspeccion, x.Codigo, x.Nombre, x.Activo);
        }

        public async Task<TipoInspeccionDto> CreateAsync(TipoInspeccionCreateDto dto)
        {
            var e = new TipoInspeccion
            {
                IdTipoInspeccion = dto.IdTipoInspeccion,
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Activo = true
            };
            _context.TiposInspeccion.Add(e);
            await _context.SaveChangesAsync();
            return new TipoInspeccionDto(e.IdTipoInspeccion, e.Codigo, e.Nombre, e.Activo);
        }

        public async Task<TipoInspeccionDto?> UpdateAsync(short id, TipoInspeccionUpdateDto dto)
        {
            var e = await _context.TiposInspeccion.FindAsync(id);
            if (e is null) return null;
            e.Nombre = dto.Nombre.Trim();
            e.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return new TipoInspeccionDto(e.IdTipoInspeccion, e.Codigo, e.Nombre, e.Activo);
        }

        public async Task<bool> DeleteAsync(short id)
        {
            var e = await _context.TiposInspeccion.FindAsync(id);
            if (e is null) return false;
            e.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
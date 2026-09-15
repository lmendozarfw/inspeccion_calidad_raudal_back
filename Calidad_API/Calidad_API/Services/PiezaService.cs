using Calidad_API.Data;
using Calidad_API.DTOs.Piezas;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class PiezaService : IPiezaService
    {
        private readonly ApplicationDbContext _context;
        public PiezaService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<PiezaDto>> GetAllAsync(bool soloActivas = true)
        {
            var q = _context.Piezas.AsNoTracking().AsQueryable();
            if (soloActivas) q = q.Where(x => x.Activo);
            return await q.OrderBy(x => x.Codigo)
                .Select(x => new PiezaDto(x.IdPieza, x.Codigo, x.Nombre, x.Unidad, x.Activo, x.FechaAlta))
                .ToListAsync();
        }

        public async Task<PiezaDto?> GetByIdAsync(long id)
        {
            var x = await _context.Piezas.AsNoTracking().FirstOrDefaultAsync(p => p.IdPieza == id);
            return x is null ? null : new PiezaDto(x.IdPieza, x.Codigo, x.Nombre, x.Unidad, x.Activo, x.FechaAlta);
        }

        public async Task<PiezaDto> CreateAsync(PiezaCreateDto dto)
        {
            var e = new Pieza
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Unidad = dto.Unidad?.Trim(),
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };
            _context.Piezas.Add(e);
            await _context.SaveChangesAsync();
            return new PiezaDto(e.IdPieza, e.Codigo, e.Nombre, e.Unidad, e.Activo, e.FechaAlta);
        }

        public async Task<PiezaDto?> UpdateAsync(long id, PiezaUpdateDto dto)
        {
            var e = await _context.Piezas.FindAsync(id);
            if (e is null) return null;
            e.Nombre = dto.Nombre.Trim();
            e.Unidad = dto.Unidad?.Trim();
            e.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return new PiezaDto(e.IdPieza, e.Codigo, e.Nombre, e.Unidad, e.Activo, e.FechaAlta);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var e = await _context.Piezas.FindAsync(id);
            if (e is null) return false;
            e.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
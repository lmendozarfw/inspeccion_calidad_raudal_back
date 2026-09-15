using Calidad_API.Data;
using Calidad_API.DTOs.Modelos;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class ModeloService : IModeloService
    {
        private readonly ApplicationDbContext _context;
        public ModeloService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<ModeloDto>> GetAllAsync(bool soloActivos = true)
        {
            var q = _context.Modelos.AsNoTracking().AsQueryable();
            if (soloActivos) q = q.Where(x => x.Activo);
            return await q.OrderBy(x => x.Codigo)
                .Select(x => new ModeloDto(x.IdModelo, x.Codigo, x.Nombre, x.Familia, x.Activo, x.FechaAlta))
                .ToListAsync();
        }

        public async Task<ModeloDto?> GetByIdAsync(long id)
        {
            var x = await _context.Modelos.AsNoTracking().FirstOrDefaultAsync(m => m.IdModelo == id);
            return x is null ? null : new ModeloDto(x.IdModelo, x.Codigo, x.Nombre, x.Familia, x.Activo, x.FechaAlta);
        }

        public async Task<ModeloDto?> GetByCodigoAsync(string codigo)
        {
            var x = await _context.Modelos.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Codigo == codigo.Trim().ToUpper());
            return x is null ? null : new ModeloDto(x.IdModelo, x.Codigo, x.Nombre, x.Familia, x.Activo, x.FechaAlta);
        }

        public async Task<ModeloDto> CreateAsync(ModeloCreateDto dto)
        {
            var e = new Modelo
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Familia = dto.Familia?.Trim(),
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };
            _context.Modelos.Add(e);
            await _context.SaveChangesAsync();
            return new ModeloDto(e.IdModelo, e.Codigo, e.Nombre, e.Familia, e.Activo, e.FechaAlta);
        }

        public async Task<ModeloDto?> UpdateAsync(long id, ModeloUpdateDto dto)
        {
            var e = await _context.Modelos.FindAsync(id);
            if (e is null) return null;
            e.Nombre = dto.Nombre.Trim();
            e.Familia = dto.Familia?.Trim();
            e.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return new ModeloDto(e.IdModelo, e.Codigo, e.Nombre, e.Familia, e.Activo, e.FechaAlta);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var e = await _context.Modelos.FindAsync(id);
            if (e is null) return false;
            e.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
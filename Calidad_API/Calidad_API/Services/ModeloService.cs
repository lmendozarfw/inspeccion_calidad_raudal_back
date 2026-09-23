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
            if (soloActivos) q = q.Where(x => x.Estatus);

            return await q
                .OrderBy(x => x.CodigoMB)
                .ThenBy(x => x.CodigoCombinacion)
                .Select(x => new ModeloDto(
                    x.IdModelo,
                    x.CodigoMB,
                    x.CodigoCombinacion,
                    x.Descripcion,
                    x.Estatus))
                .ToListAsync();
        }

        public async Task<ModeloDto?> GetByIdAsync(int id)
        {
            var x = await _context.Modelos.AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdModelo == id);
            return x is null ? null : Map(x);
        }

        public async Task<ModeloDto?> GetByCodigosAsync(string codigoModeloBase, string codigoCombinacion)
        {
            var baseCode = codigoModeloBase.Trim().ToUpper();
            var comb = codigoCombinacion.Trim().ToUpper();

            var x = await _context.Modelos.AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.CodigoMB == baseCode &&
                    m.CodigoCombinacion == comb);

            return x is null ? null : Map(x);
        }

        public async Task<ModeloDto> CreateAsync(ModeloCreateDto dto)
        {
            var e = new Modelo
            {
                CodigoMB = dto.CodigoModeloBase.Trim().ToUpper(),
                CodigoCombinacion = dto.CodigoCombinacion.Trim().ToUpper(),
                Descripcion = dto.Descripcion.Trim(),
                Estatus = true
            };
            _context.Modelos.Add(e);
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<ModeloDto?> UpdateAsync(int id, ModeloUpdateDto dto)
        {
            var e = await _context.Modelos.FindAsync(id);
            if (e is null) return null;

            e.CodigoMB = dto.CodigoModeloBase.Trim().ToUpper();
            e.CodigoCombinacion = dto.CodigoCombinacion.Trim().ToUpper();
            e.Descripcion = dto.Descripcion.Trim();
            e.Estatus = dto.Estatus;

            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _context.Modelos.FindAsync(id);
            if (e is null) return false;
            e.Estatus = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ModeloDto Map(Modelo x) => new(
            x.IdModelo,
            x.CodigoMB,
            x.CodigoCombinacion,
            x.Descripcion,
            x.Estatus);
    }
}
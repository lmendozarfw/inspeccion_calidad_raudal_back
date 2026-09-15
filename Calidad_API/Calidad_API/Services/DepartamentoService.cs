using Calidad_API.Data;
using Calidad_API.DTOs.Departamentos;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly ApplicationDbContext _context;
        public DepartamentoService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<DepartamentoDto>> GetAllAsync(bool soloActivos = true)
        {
            var q = _context.Departamentos.AsNoTracking().Include(d => d.UnidadNegocio).AsQueryable();
            if (soloActivos) q = q.Where(x => x.Activo);
            return await q
                .OrderBy(x => x.Codigo)
                .Select(d => new DepartamentoDto(
                    d.IdDepartamento,
                    d.Codigo,
                    d.Nombre,
                    d.IdUnidadNegocio,
                    d.UnidadNegocio != null ? d.UnidadNegocio.Nombre : null,
                    d.Activo,
                    d.FechaAlta))
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartamentoDto>> GetByUnidadNegocioAsync(long idUnidadNegocio)
        {
            return await _context.Departamentos.AsNoTracking()
                .Include(d => d.UnidadNegocio)
                .Where(x => x.Activo && x.IdUnidadNegocio == idUnidadNegocio)
                .OrderBy(x => x.Codigo)
                .Select(d => new DepartamentoDto(
                    d.IdDepartamento,
                    d.Codigo,
                    d.Nombre,
                    d.IdUnidadNegocio,
                    d.UnidadNegocio != null ? d.UnidadNegocio.Nombre : null,
                    d.Activo,
                    d.FechaAlta))
                .ToListAsync();
        }

        public async Task<DepartamentoDto?> GetByIdAsync(long id)
        {
            var x = await _context.Departamentos.AsNoTracking()
                .Include(d => d.UnidadNegocio)
                .FirstOrDefaultAsync(d => d.IdDepartamento == id);
            return x is null ? null : Map(x);
        }

        public async Task<DepartamentoDto> CreateAsync(DepartamentoCreateDto dto)
        {
            var e = new Departamento
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                IdUnidadNegocio = dto.IdUnidadNegocio,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };
            _context.Departamentos.Add(e);
            await _context.SaveChangesAsync();
            await _context.Entry(e).Reference(x => x.UnidadNegocio).LoadAsync();
            return Map(e);
        }

        public async Task<DepartamentoDto?> UpdateAsync(long id, DepartamentoUpdateDto dto)
        {
            var e = await _context.Departamentos.FindAsync(id);
            if (e is null) return null;
            e.Nombre = dto.Nombre.Trim();
            e.IdUnidadNegocio = dto.IdUnidadNegocio;
            e.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            await _context.Entry(e).Reference(x => x.UnidadNegocio).LoadAsync();
            return Map(e);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var e = await _context.Departamentos.FindAsync(id);
            if (e is null) return false;
            e.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static DepartamentoDto Map(Departamento d) => new(
            d.IdDepartamento, d.Codigo, d.Nombre, d.IdUnidadNegocio,
            d.UnidadNegocio?.Nombre, d.Activo, d.FechaAlta);
    }
}
using Calidad_API.Data;
using Calidad_API.DTOs.Area;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;
namespace Calidad_API.Services
{
    public class AreaService : IAreaService
    {
        private readonly ApplicationDbContext _context;

        public AreaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AreaDto>> GetAllAsync(bool soloActivas = true)
        {
            var query = _context.Areas.AsNoTracking();
            if (soloActivas) query = query.Where(a => a.Activo);

            return await query
                .OrderBy(a => a.Proceso)
                .ThenBy(a => a.Codigo)
                .Select(a => new AreaDto(
                    a.IdArea, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdCentroTrabajo, a.IdDepartamento))
                .ToListAsync();
        }

        public async Task<AreaDto?> GetByIdAsync(long id)
        {
            var a = await _context.Areas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdArea == id);
            if (a is null) return null;

            return new AreaDto(a.IdArea, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                a.IdCentroTrabajo, a.IdDepartamento);
        }

        public async Task<IEnumerable<AreaDto>> GetByProcesoAsync(string proceso)
        {
            return await _context.Areas.AsNoTracking()
                .Where(a => a.Activo && a.Proceso == proceso)
                .OrderBy(a => a.Codigo)
                .Select(a => new AreaDto(
                    a.IdArea, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdCentroTrabajo, a.IdDepartamento))
                .ToListAsync();
        }

        public async Task<AreaDto> CreateAsync(AreaCreateDto dto)
        {
            var entity = new Area
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Proceso = dto.Proceso.Trim().ToUpper(),
                IdCentroTrabajo = dto.IdCentroTrabajo,
                IdDepartamento = dto.IdDepartamento,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Areas.Add(entity);
            await _context.SaveChangesAsync();

            return new AreaDto(entity.IdArea, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdCentroTrabajo, entity.IdDepartamento);
        }

        public async Task<AreaDto?> UpdateAsync(long id, AreaUpdateDto dto)
        {
            var entity = await _context.Areas.FindAsync(id);
            if (entity is null) return null;

            entity.Nombre = dto.Nombre.Trim();
            entity.Proceso = dto.Proceso.Trim().ToUpper();
            entity.Activo = dto.Activo;
            entity.IdCentroTrabajo = dto.IdCentroTrabajo;
            entity.IdDepartamento = dto.IdDepartamento;

            await _context.SaveChangesAsync();

            return new AreaDto(entity.IdArea, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdCentroTrabajo, entity.IdDepartamento);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Areas.FindAsync(id);
            if (entity is null) return false;

            entity.Activo = false; // soft delete
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

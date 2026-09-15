using Calidad_API.Data;
using Calidad_API.DTOs.Area;          // o DTOs.Operaciones si ya lo renombraste
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class OperacionService : IAreaService   // o IOperacionService
    {
        private readonly ApplicationDbContext _context;

        public OperacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AreaDto>> GetAllAsync(bool soloActivas = true)
        {
            var query = _context.Operaciones.AsNoTracking();
            if (soloActivas) query = query.Where(a => a.Activo);

            return await query
                .OrderBy(a => a.Proceso)
                .ThenBy(a => a.Codigo)
                .Select(a => new AreaDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio))
                .ToListAsync();
        }

        public async Task<AreaDto?> GetByIdAsync(long id)
        {
            var a = await _context.Operaciones.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdOperacion == id);
            if (a is null) return null;

            return new AreaDto(a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                a.IdDepartamento, a.IdUnidadNegocio);
        }

        public async Task<IEnumerable<AreaDto>> GetByProcesoAsync(string proceso)
        {
            return await _context.Operaciones.AsNoTracking()
                .Where(a => a.Activo && a.Proceso == proceso)
                .OrderBy(a => a.Codigo)
                .Select(a => new AreaDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio))
                .ToListAsync();
        }

        public async Task<AreaDto> CreateAsync(AreaCreateDto dto)
        {
            var entity = new Operacion
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Proceso = dto.Proceso.Trim().ToUpper(),
                IdDepartamento = dto.IdCentroTrabajo,      // mapear según tu DTO
                IdUnidadNegocio = dto.IdDepartamento,      // mapear según tu DTO
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Operaciones.Add(entity);
            await _context.SaveChangesAsync();

            return new AreaDto(entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdDepartamento, entity.IdUnidadNegocio);
        }

        public async Task<AreaDto?> UpdateAsync(long id, AreaUpdateDto dto)
        {
            var entity = await _context.Operaciones.FindAsync(id);
            if (entity is null) return null;

            entity.Nombre = dto.Nombre.Trim();
            entity.Proceso = dto.Proceso.Trim().ToUpper();
            entity.Activo = dto.Activo;
            entity.IdDepartamento = dto.IdCentroTrabajo;
            entity.IdUnidadNegocio = dto.IdDepartamento;

            await _context.SaveChangesAsync();

            return new AreaDto(entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdDepartamento, entity.IdUnidadNegocio);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Operaciones.FindAsync(id);
            if (entity is null) return false;

            entity.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
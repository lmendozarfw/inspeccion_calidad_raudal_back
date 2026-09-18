using Calidad_API.Data;
using Calidad_API.DTOs.Operaciones;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class OperacionService : IOperacionService
    {
        private readonly ApplicationDbContext _context;

        public OperacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OperacionDto>> GetAllAsync(bool soloActivas = true)
        {
            var query = _context.Operaciones.AsNoTracking();
            if (soloActivas) query = query.Where(a => a.Activo);

            return await query
                .OrderBy(a => a.Proceso)
                .ThenBy(a => a.Codigo)
                .Select(a => new OperacionDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio))
                .ToListAsync();
        }

        public async Task<OperacionDto?> GetByIdAsync(long id)
        {
            var a = await _context.Operaciones.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdOperacion == id);
            if (a is null) return null;

            return new OperacionDto(
                a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                a.IdDepartamento, a.IdUnidadNegocio);
        }

        public async Task<IEnumerable<OperacionDto>> GetByProcesoAsync(string proceso)
        {
            return await _context.Operaciones.AsNoTracking()
                .Where(a => a.Activo && a.Proceso == proceso.ToUpper())
                .OrderBy(a => a.Codigo)
                .Select(a => new OperacionDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio))
                .ToListAsync();
        }

        public async Task<IEnumerable<OperacionDto>> GetByDepartamentoAsync(long idDepartamento)
        {
            return await _context.Operaciones.AsNoTracking()
                .Where(a => a.Activo && a.IdDepartamento == idDepartamento)
                .OrderBy(a => a.Codigo)
                .Select(a => new OperacionDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio))
                .ToListAsync();
        }

        public async Task<IEnumerable<OperacionPermisoDto>> GetMisPermisosAsync(long idUsuario)
        {
            return await _context.PermisosOperacion.AsNoTracking()
                .Include(p => p.Operacion)
                .Where(p => p.IdUsuario == idUsuario && p.Operacion.Activo)
                .OrderBy(p => p.Operacion.Proceso)
                .ThenBy(p => p.Operacion.Codigo)
                .Select(p => new OperacionPermisoDto(
                    p.IdOperacion,
                    p.Operacion.Codigo,
                    p.Operacion.Nombre,
                    p.Operacion.Proceso,
                    p.PuedeCapturar,
                    p.PuedeConsultar,
                    p.PuedeGenerarVale))
                .ToListAsync();
        }

        public async Task<OperacionDto> CreateAsync(OperacionCreateDto dto)
        {
            var entity = new Operacion
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Proceso = dto.Proceso.Trim().ToUpper(),
                IdDepartamento = dto.IdDepartamento,
                IdUnidadNegocio = dto.IdUnidadNegocio,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Operaciones.Add(entity);
            await _context.SaveChangesAsync();

            return new OperacionDto(
                entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo,
                entity.IdDepartamento, entity.IdUnidadNegocio);
        }

        public async Task<OperacionDto?> UpdateAsync(long id, OperacionUpdateDto dto)
        {
            var entity = await _context.Operaciones.FindAsync(id);
            if (entity is null) return null;

            entity.Nombre = dto.Nombre.Trim();
            entity.Proceso = dto.Proceso.Trim().ToUpper();
            entity.Activo = dto.Activo;
            entity.IdDepartamento = dto.IdDepartamento;
            entity.IdUnidadNegocio = dto.IdUnidadNegocio;

            await _context.SaveChangesAsync();

            return new OperacionDto(
                entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo,
                entity.IdDepartamento, entity.IdUnidadNegocio);
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
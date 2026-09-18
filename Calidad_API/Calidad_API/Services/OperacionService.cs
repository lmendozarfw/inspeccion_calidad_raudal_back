using Calidad_API.Data;
using Calidad_API.DTOs.Area;
using Calidad_API.DTOs.Operaciones;
using Calidad_API.Interfaces; // o DTOs.Operaciones si ya lo renombraste
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class OperacionService : IOperacionService   // o IOperacionService
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
                .Include(a => a.Departamento)
                .Select(a => new OperacionDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio, a.Departamento != null ? a.Departamento.Nombre : ""))
                .ToListAsync();
        }

        public async Task<OperacionDto?> GetByIdAsync(long id)
        {
            var a = await _context.Operaciones.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdOperacion == id);
            if (a is null) return null;
            var departamento = await _context.Departamentos.AsNoTracking().FirstOrDefaultAsync(d => d.IdDepartamento == a.IdDepartamento);
            return new OperacionDto(a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                a.IdDepartamento, a.IdUnidadNegocio,  departamento != null ? departamento.Nombre : "");
        }

        public async Task<IEnumerable<OperacionDto>> GetByProcesoAsync(string proceso)
        {
            return await _context.Operaciones.AsNoTracking()
                .Where(a => a.Activo && a.Proceso == proceso)
                .OrderBy(a => a.Codigo)
                .Include(a => a.Departamento)
                .Select(a => new OperacionDto(
                    a.IdOperacion, a.Codigo, a.Nombre, a.Proceso, a.Activo,
                    a.IdDepartamento, a.IdUnidadNegocio, a.Departamento != null ? a.Departamento.Nombre : ""))
                .ToListAsync();
        }

        public Task<IEnumerable<OperacionDto>> GetByDepartamentoAsync(long idDepartamento)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OperacionPermisoDto>> GetMisPermisosAsync(long idUsuario)
        {
            throw new NotImplementedException();
        }

        public async Task<OperacionDto> CreateAsync(OperacionCreateDto dto)
        {
            var entity = new Operacion
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Proceso = dto.Proceso.Trim().ToUpper(),
                IdDepartamento = dto.IdDepartamento,
                IdUnidadNegocio = dto.IdDepartamento,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Operaciones.Add(entity);
            await _context.SaveChangesAsync();
            var departamento = await _context.Departamentos.AsNoTracking().FirstOrDefaultAsync(d => d.IdDepartamento == dto.IdDepartamento);
            return new OperacionDto(entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdDepartamento, entity.IdUnidadNegocio, departamento  != null ? departamento.Nombre : "");
        }

        public async Task<OperacionDto?> UpdateAsync(long id, OperacionUpdateDto dto)
        {
            var entity = await _context.Operaciones.FindAsync(id);
            if (entity is null) return null;

            entity.Nombre = dto.Nombre.Trim();
            entity.Proceso = dto.Proceso.Trim().ToUpper();
            entity.Activo = dto.Activo;
            entity.IdDepartamento = dto.IdDepartamento;
            entity.IdUnidadNegocio = dto.IdDepartamento;

            await _context.SaveChangesAsync();
            var departamento = await _context.Departamentos.AsNoTracking().FirstOrDefaultAsync(d => d.IdDepartamento == dto.IdDepartamento);
            return new OperacionDto(entity.IdOperacion, entity.Codigo, entity.Nombre,
                entity.Proceso, entity.Activo, entity.IdDepartamento, entity.IdUnidadNegocio, departamento != null ? departamento.Nombre : "");
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
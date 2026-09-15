using Calidad_API.Data;
using Calidad_API.DTOs.Defectos;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class DefectoService : IDefectoService
    {
        private readonly ApplicationDbContext _context;

        public DefectoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DefectoDto>> GetByAreaAsync(long IdOperacion, bool soloActivos = true)
        {
            var query = _context.Defectos
                .AsNoTracking()
                .Include(d => d.Criticidad)
                .Include(d => d.Pieza)
                .Where(d => d.IdOperacion == IdOperacion);

            if (soloActivos) query = query.Where(d => d.Activo);

            return await query
                .OrderBy(d => d.Codigo)
                .Select(d => new DefectoDto(
                    d.IdDefecto,
                    d.IdOperacion,
                    d.Codigo,
                    d.Nombre,
                    d.IdCriticidad,
                    d.Criticidad != null ? d.Criticidad.Codigo : null,
                    d.AplicaPieza,
                    d.IdPieza,
                    d.Pieza != null ? d.Pieza.Codigo : null,
                    d.Ponderacion,
                    d.Activo))
                .ToListAsync();
        }

        public async Task<DefectoDto?> GetByIdAsync(long id)
        {
            var d = await _context.Defectos
                .AsNoTracking()
                .Include(x => x.Criticidad)
                .Include(x => x.Pieza)
                .FirstOrDefaultAsync(x => x.IdDefecto == id);

            if (d is null) return null;

            return new DefectoDto(
                d.IdDefecto, d.IdOperacion, d.Codigo, d.Nombre,
                d.IdCriticidad, d.Criticidad?.Codigo,
                d.AplicaPieza, d.IdPieza, d.Pieza?.Codigo,
                d.Ponderacion, d.Activo);
        }

        public async Task<DefectoDto> CreateAsync(DefectoCreateDto dto)
        {
            // Validar que el área exista
            var areaExiste = await _context.Operaciones.AnyAsync(a => a.IdOperacion == dto.IdOperacion && a.Activo);
            if (!areaExiste)
                throw new InvalidOperationException("El área no existe o está inactiva.");

            var entity = new Defecto
            {
                IdOperacion = dto.IdOperacion,
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                IdCriticidad = dto.IdCriticidad,
                AplicaPieza = dto.AplicaPieza,
                IdPieza = dto.AplicaPieza ? dto.IdPieza : null,
                Ponderacion = dto.Ponderacion,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Defectos.Add(entity);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(entity.IdDefecto))!;
        }

        public async Task<DefectoDto?> UpdateAsync(long id, DefectoUpdateDto dto)
        {
            var entity = await _context.Defectos.FindAsync(id);
            if (entity is null) return null;

            entity.Nombre = dto.Nombre.Trim();
            entity.IdCriticidad = dto.IdCriticidad;
            entity.AplicaPieza = dto.AplicaPieza;
            entity.IdPieza = dto.AplicaPieza ? dto.IdPieza : null;
            entity.Ponderacion = dto.Ponderacion;
            entity.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Defectos.FindAsync(id);
            if (entity is null) return false;

            entity.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

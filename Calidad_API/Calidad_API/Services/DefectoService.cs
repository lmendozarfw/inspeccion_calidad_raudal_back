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

        public async Task<IEnumerable<DefectoDto>> GetAllAsync(DefectoFiltroDto? filtros)
        {
            var query = _context.Defectos
                .AsNoTracking()
                .Include(d => d.Criticidad)
                .Include(d => d.Pieza)
                .Include(d => d.DefectosOperacion).ThenInclude(dop => dop.Operacion)
                .Include(d => d.DefectoTiposInspeccion).ThenInclude(di => di.TipoInspeccion).AsQueryable();
            if (filtros != null)
            {
                
                if (filtros.OperationsIds != null && filtros.OperationsIds.Any())
                {
                    query = query.Where(d =>
                        d.DefectosOperacion.Any(dop =>
                            filtros.OperationsIds.Contains(dop.IdOperacion)
                        )
                    );
                }
            
                if (filtros.InspectionTypeIds != null && filtros.InspectionTypeIds.Any())
                {
                    query = query.Where(d =>
                        d.DefectoTiposInspeccion.Any(dop =>
                            filtros.InspectionTypeIds.Contains(dop.IdTipoInspeccion)
                        )
                    );
                }
            
                if (filtros.CriticalityIds != null && filtros.CriticalityIds.Any())
                {
                    query = query.Where(d =>
                        _context.Criticidades.Any(c =>
                            filtros.CriticalityIds.Contains(c.IdCriticidad)
                        )
                    );
                }
            }
            
            return await query
                .OrderBy(d => d.Codigo)
                .Select(d => new DefectoDto(
                    d.IdDefecto,
                    d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.IdOperacion).ToList(),
                    d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.Operacion.Nombre).ToList(),
                    d.Codigo,
                    d.Nombre,
                    d.Criticidad.IdCriticidad,
                    d.Criticidad.Codigo,
                    d.Criticidad.Nombre,
                    d.AplicaPieza,
                    d.IdPieza,
                    d.Pieza.Codigo,
                    d.Pieza.Nombre,
                    d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.IdTipoInspeccion).ToList(),
                    d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.TipoInspeccion.Nombre).ToList(),
                    d.Ponderacion,
                    d.Activo))
                .ToListAsync();
        }

        public async Task<IEnumerable<DefectoDto>> GetByAreaAsync(long IdOperacion, bool soloActivos = true)
        {
            return [];
        }

        public async Task<IEnumerable<DefectoDto>> GetByOperationAndInspectionType(long idOperacion, long idTipoInspeccion)
        {
            var query = _context.Defectos.AsNoTracking().Include(d => d.Criticidad)
                .Include(d => d.Pieza)
                .Include(d => d.DefectoTiposInspeccion).ThenInclude(di => di.TipoInspeccion)
                .Include(d => d.DefectosOperacion).ThenInclude(dop => dop.Operacion)
                .Where(d =>
                d.DefectosOperacion.Any(dop => dop.IdOperacion == idOperacion) &&
                d.DefectoTiposInspeccion.Any(dt => dt.IdTipoInspeccion == idTipoInspeccion)).AsQueryable();
            return await query.OrderBy(d => d.Codigo).Select(d => new DefectoDto(
                d.IdDefecto,
                d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.IdOperacion).ToList(),
                d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.Operacion.Nombre).ToList(),
                d.Codigo,
                d.Nombre,
                d.IdCriticidad,
                d.Criticidad != null ? d.Criticidad.Codigo : null,
                d.Criticidad != null ? d.Criticidad.Nombre : null,
                d.AplicaPieza,
                d.IdPieza,
                d.Pieza != null ? d.Pieza.Codigo : null,
                d.Pieza != null ? d.Pieza.Nombre : null,
                d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.IdTipoInspeccion)
                    .ToList(),
                d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.TipoInspeccion.Nombre)
                    .ToList(),
                d.Ponderacion,
                d.Activo)).ToListAsync();
        }

        public async Task<DefectoDto?> GetByIdAsync(long id)
        {
            var d = await _context.Defectos
                .AsNoTracking()
                .Include(x => x.DefectosOperacion).ThenInclude(dop => dop.Operacion)
                .Include(x => x.Criticidad)
                .Include(x => x.Pieza)
                .Include(x => x.DefectoTiposInspeccion).ThenInclude(di => di.TipoInspeccion)
                .FirstOrDefaultAsync(x => x.IdDefecto == id);

            if (d is null) return null;

            return new DefectoDto(
                d.IdDefecto, 
                d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.IdOperacion).ToList(),
                d.DefectosOperacion.OrderBy(dop => dop.Operacion.Nombre).Select(dop => dop.Operacion.Nombre).ToList(), 
                d.Codigo, 
                d.Nombre,
                d.IdCriticidad, d.Criticidad?.Codigo, d.Criticidad?.Nombre,
                d.AplicaPieza, d.IdPieza, d.Pieza?.Codigo, d.Pieza?.Nombre,
                d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.IdTipoInspeccion).ToList(),
                d.DefectoTiposInspeccion.OrderBy(di => di.TipoInspeccion.Nombre).Select(di => di.TipoInspeccion.Nombre).ToList(),
                d.Ponderacion, d.Activo);
        }

        public async Task<DefectoDto> CreateAsync(DefectoCreateDto dto)
        {
            var operaciones = await _context.Operaciones.Where(x => dto.IdsOperacion.Contains(x.IdOperacion))
                .ToListAsync();
            if (operaciones.Count != dto.IdsOperacion.Count)
            {
                throw new BadHttpRequestException(
                    "Una o más operaciones no existen."
                );
            }
            
            var tiposInspeccion = await _context.TiposInspeccion
                .Where(x => dto.IdsTipoInspeccion.Contains(x.IdTipoInspeccion))
                .ToListAsync();

            if (tiposInspeccion.Count != dto.IdsTipoInspeccion.Count)
            {
                throw new BadHttpRequestException(
                    "Uno o más tipos de inspección no existen."
                );
            }

            var entity = new Defecto
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                IdCriticidad = dto.IdCriticidad,
                AplicaPieza = dto.AplicaPieza,
                IdPieza = dto.AplicaPieza ? dto.IdPieza : null,
                Ponderacion = dto.Ponderacion,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            foreach (var operacion in operaciones)
            {
                entity.DefectosOperacion.Add(new DefectoOperacion
                {
                   IdOperacion = operacion.IdOperacion,
                });
            }
            
            foreach (var tipo in tiposInspeccion)
            {
                entity.DefectoTiposInspeccion.Add(
                    new DefectoTipoInspeccion
                    {
                        IdTipoInspeccion = tipo.IdTipoInspeccion
                    }
                );
            }
            
            _context.Defectos.Add(entity);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(entity.IdDefecto))!;
        }

        public async Task<DefectoDto?> UpdateAsync(long id, DefectoUpdateDto dto)
        {
            var entity = await _context.Defectos
                .Include(x => x.DefectosOperacion)
                .Include(x => x.DefectoTiposInspeccion)
                .FirstOrDefaultAsync(x => x.IdDefecto == id);
            if (entity is null) return null;

            var operaciones = await _context.Operaciones.Where(x => dto.IdsOperacion.Contains(x.IdOperacion))
                .ToListAsync();
            if (operaciones.Count != dto.IdsOperacion.Count)
            {
                throw new BadHttpRequestException(
                    "Una o más operaciones no existen."
                );
            }
            
            var tiposInspeccion = await _context.TiposInspeccion
                .Where(x => dto.IdsTipoInspeccion.Contains(x.IdTipoInspeccion))
                .ToListAsync();

            if (tiposInspeccion.Count != dto.IdsTipoInspeccion.Count)
            {
                throw new BadHttpRequestException(
                    "Uno o más tipos de inspección no existen."
                );
            }
            
            entity.Nombre = dto.Nombre.Trim();
            entity.IdCriticidad = dto.IdCriticidad;
            entity.AplicaPieza = dto.AplicaPieza;
            entity.IdPieza = dto.AplicaPieza ? dto.IdPieza : null;
            entity.Ponderacion = dto.Ponderacion;
            entity.Activo = dto.Activo;

            _context.DefectosOperacion.RemoveRange(entity.DefectosOperacion);
            _context.DefectoTiposInspeccion.RemoveRange(entity.DefectoTiposInspeccion);

            foreach (var operacion in operaciones)
            {
                entity.DefectosOperacion.Add(new DefectoOperacion
                {
                    IdOperacion = operacion.IdOperacion,
                });
            }

            foreach (var tipo in tiposInspeccion)
            {
                entity.DefectoTiposInspeccion.Add(
                    new DefectoTipoInspeccion
                    {
                        IdTipoInspeccion = tipo.IdTipoInspeccion
                    }
                );
            }
            
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

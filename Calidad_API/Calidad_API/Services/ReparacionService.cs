using Calidad_API.Data;
using Calidad_API.DTOs.Inspecciones;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services;

public class ReparacionService : IReparacionService
{

    private readonly ApplicationDbContext _context;
    public ReparacionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReparacionDto?> CreateAsync(ReparacionCreateDto request, long idUsuario)
    {
        var reparacion = new Reparacion
        {
            IdUsuario = idUsuario,
            IdInspeccion = request.IdInspeccion,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin
        };
        await _context.Reparaciones.AddAsync(reparacion);
        await _context.SaveChangesAsync();
        return await _context.Reparaciones.AsNoTracking()
        .Where(x => x.IdReparacion == reparacion.IdReparacion)
        .Select(x => new ReparacionDto(
            x.IdReparacion,
            x.IdUsuario,
            x.Usuario.Nombre,
            x.Usuario.Username,
            x.IdInspeccion,
            x.FechaInicio, x.FechaFin,
            x.Inspeccion.Detalles
                        .Where(d => d.TipoRegistro == "PIOCHA")
                        .Select(d => new InspeccionDetalleDto(
                            d.IdDetalle,
                            d.IdDefecto,
                            d.Defecto.Codigo,
                            d.Defecto.Nombre,
                            d.TipoRegistro,
                            d.Lado,
                            d.Cantidad,
                            d.ValorObjetivo,
                            d.ValorMin,
                            d.ValorMax,
                            d.Unidad,
                            d.ValorObtenido,
                            d.ResultadoCualitativo,
                            d.FechaRegistro
                        ))
                        .ToList()
            ))
        .FirstOrDefaultAsync();

    }

    public async Task<List<ReparacionDto>> GetByAllAsync(long idInspeccion)
    {
        return await _context.Reparaciones
        .AsNoTracking()
        .Where(x => x.IdInspeccion == idInspeccion)
        .Include(x => x.Usuario)
        .Include(x => x.Inspeccion)
        .ThenInclude(x => x.Detalles)
        .OrderByDescending(x => x.FechaInicio)
        .Select(x => new ReparacionDto(
            x.IdReparacion,
            x.IdUsuario,
            x.Usuario.Nombre,
            x.Usuario.Username,
            x.IdInspeccion,
            x.FechaInicio,
            x.FechaFin,
             x.Inspeccion.Detalles
                        .Where(d => d.TipoRegistro == "PIOCHA")
                        .Select(d => new InspeccionDetalleDto(
                            d.IdDetalle,
                            d.IdDefecto,
                            d.Defecto.Codigo,
                            d.Defecto.Nombre,
                            d.TipoRegistro,
                            d.Lado,
                            d.Cantidad,
                            d.ValorObjetivo,
                            d.ValorMin,
                            d.ValorMax,
                            d.Unidad,
                            d.ValorObtenido,
                            d.ResultadoCualitativo,
                            d.FechaRegistro
                        ))
                        .ToList()
             ))
        .ToListAsync();
    }
}
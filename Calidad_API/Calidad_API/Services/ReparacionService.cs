using Calidad_API.Data;
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
        var reparacion = new Reparacion{
            IdUsuario = idUsuario,
            IdInspeccionDetalle = request.IdInspeccionDetalle,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin
        };
        await _context.Reparaciones.AddAsync(reparacion);
        await _context.SaveChangesAsync();
        return await _context.Reparaciones.AsNoTracking()
        .Where(x => x.IdReparacion == reparacion.IdReparacion)
        .Select(x => new ReparacionDto(
            x.IdReparacion, x.IdUsuario, x.Usuario.Nombre, x.Usuario.Username,
            x.IdInspeccionDetalle, x.InspeccionDetalle.Defecto.Nombre,
            x.FechaInicio, x.FechaFin))
        .FirstOrDefaultAsync();

    }

    public async Task<List<ReparacionDto>> GetByAllAsync(long idDetalle)
    {
        return await _context.Reparaciones
        .AsNoTracking()
        .Where(x => x.IdInspeccionDetalle == idDetalle)
        .Include(x => x.Usuario)
        .Include(x => x.InspeccionDetalle)
        .ThenInclude(x => x.Defecto)
        .OrderByDescending(x => x.FechaInicio)
        .Select(x => new ReparacionDto(
            x.IdReparacion,
            x.IdUsuario, 
            x.Usuario.Nombre,
            x.Usuario.Username,
            x.IdInspeccionDetalle, 
            x.InspeccionDetalle.Defecto.Nombre,
            x.FechaInicio,
            x.FechaFin ))
        .ToListAsync();
    }
}
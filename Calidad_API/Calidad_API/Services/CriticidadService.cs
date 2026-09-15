using Calidad_API.Data;
using Calidad_API.DTOs.Criticidad;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services;

public class CriticidadService : ICriticidadService
{
    private readonly ApplicationDbContext _context;

    public CriticidadService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CriticidadDto>> GetAllAsync()
    {
        return await _context.Criticidades.OrderBy(c => c.Nombre).Select(c => new CriticidadDto(
            c.IdCriticidad,
            c.Nombre,
            c.Codigo,
            c.Nivel
        )).ToListAsync();
    }
}
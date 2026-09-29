using Calidad_API.Data;
using Calidad_API.DTOs.Dashboard;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services;

public class DashboardService : IDashboardService
{
    private const string CriticidadCriticaCodigo = "CRITICO";
    private const int TopDefectosLimite = 10;
    private const string SerieDefectosNombre = "Defectos";
    private const string FormatoFecha = "yyyy-MM-dd";

    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetAll(DashboardDateRange range)
    {
        var desde = range.StartDate;
        var hasta = range.EndDate;

        var defectos = _context.InspeccionDetalles.AsNoTracking()
            .Where(id => id.FechaRegistro >= desde && id.FechaRegistro <= hasta);

        var inspecciones = _context.Inspecciones.AsNoTracking()
            .Where(id => id.FechaInspeccion >= desde && id.FechaInspeccion <= hasta);

        var numeroDefectosEncontrados = await defectos.CountAsync();

        var nombresOperaciones = await inspecciones
            .GroupBy(id => new { id.IdOperacion, id.Operacion.Nombre })
            .Select(g => g.Key.Nombre)
            .OrderBy(nombre => nombre)
            .ToListAsync();

        var operaciones = new OperacionesRegistradasDto
        {
            Valor = nombresOperaciones.Count,
            Operaciones = nombresOperaciones
        };

        var topDefecto = await defectos
            .GroupBy(id => new { id.IdDefecto, Defecto = id.Defecto.Nombre })
            .Select(g => new { g.Key.IdDefecto, g.Key.Defecto, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.IdDefecto)
            .FirstOrDefaultAsync();

        var topDefectoOperacion = topDefecto is null
            ? null
            : await defectos
                .Where(id => id.IdDefecto == topDefecto.IdDefecto)
                .GroupBy(id => id.Inspeccion.Operacion.Nombre)
                .Select(g => new { Operacion = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.Operacion)
                .FirstOrDefaultAsync();

        var defectoMasEncontrado = new DefectoMasEncontradoDto
        {
            Valor = topDefecto?.Total ?? 0,
            Defecto = topDefecto?.Defecto ?? string.Empty,
            Operacion = topDefectoOperacion?.Operacion ?? string.Empty
        };

        var defectosCriticos = await defectos
            .CountAsync(id => id.Defecto!.Criticidad!.Codigo == CriticidadCriticaCodigo);

        var programas = await inspecciones
            .Where(id => id.Transfer.Programa != null)
            .GroupBy(id => id.Transfer.Programa)
            .Select(g => new { Nombre = g.Key, Valor = g.Count() })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var dashboardProgramas = new ProgramasDto
        {
            Valor = programas.Count,
            TopNombre = programas.FirstOrDefault()?.Nombre ?? string.Empty,
            TopValor = programas.FirstOrDefault()?.Valor ?? 0
        };

        var lotes = await inspecciones
            .GroupBy(id => id.Transfer.Lote)
            .Select(g => new { Nombre = g.Key, Valor = g.Count() })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var dashboardLotes = new LotesDto
        {
            Valor = lotes.Count,
            TopNombre = lotes.FirstOrDefault()?.Nombre ?? string.Empty,
            TopValor = lotes.FirstOrDefault()?.Valor ?? 0
        };

        var defectosPorOperacion = await defectos
            .GroupBy(id => id.Inspeccion.Operacion.Nombre)
            .Select(g => new NombreValorDto { Nombre = g.Key, Valor = g.Count() })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var defectosPorTipoInspeccion = await defectos
            .GroupBy(id => id.Inspeccion.TipoInspeccion.Nombre)
            .Select(g => new NombreValorDto { Nombre = g.Key, Valor = g.Count() })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var topDefectos = await defectos
            .GroupBy(id => new { id.IdDefecto, Defecto = id.Defecto.Nombre })
            .Select(g => new NombreValorDto { Nombre = g.Key.Defecto, Valor = g.Count() })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Nombre)
            .Take(TopDefectosLimite)
            .ToListAsync();

        var serieTiempo = (await defectos
                .GroupBy(id => id.FechaRegistro.Date)
                .Select(g => new { Fecha = g.Key, Valor = g.Count() })
                .OrderBy(x => x.Fecha)
                .ToListAsync())
            .Select(x => new NombreValorDto
            {
                Nombre = x.Fecha.ToString(FormatoFecha),
                Valor = x.Valor
            })
            .ToList();

        return new DashboardDto
        {
            DefectosRegistrados = new DefectosRegistradosDto { Valor = numeroDefectosEncontrados },
            Operaciones = operaciones,
            DefectoMasEncontrado = defectoMasEncontrado,
            DefectosCriticos = new DefectosCriticosDto { Valor = defectosCriticos },
            Programas = dashboardProgramas,
            Lotes = dashboardLotes,
            DefectosPorOperacion = defectosPorOperacion,
            DefectosPorTipoInspeccion = defectosPorTipoInspeccion,
            TopDefectos = topDefectos,
            defectosPorTiempo = new SerieNombreValorDto
            {
                Nombre = SerieDefectosNombre,
                Series = serieTiempo
            }
        };
    }
}

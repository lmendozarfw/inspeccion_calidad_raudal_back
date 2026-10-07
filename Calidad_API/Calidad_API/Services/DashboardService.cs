using Calidad_API.Data;
using Calidad_API.DTOs.Dashboard;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using DocumentFormat.OpenXml.Math;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services;

public class DashboardService : IDashboardService
{
    private const string CriticidadCriticaCodigo = "CRITICO";
    private const int TopDefectosLimite = 10;
    private const int topPorOperacion = 10;
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

        var defectos = _context.InspeccionDetalles.Include(x => x.Defecto).ThenInclude(d => d.Criticidad).AsNoTracking()
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

        var defectosCriticidad = defectos
            .GroupBy(x => x.Defecto!.IdCriticidad)
            .Select(g => new CriticidadDefecto()
            {
                Nombre = g.First().Defecto!.Criticidad!.Nombre,
                Valor = g.Count(),
                Percentage = $"{(double)g.Count() * 100 / defectos.Count():0.00}%"
            }).ToList();

        var defectosPorLado = defectos.GroupBy(x => x.Lado)
                                .Select(g => new NombreValorDto()
                                {
                                    Nombre = g.First().Lado!,
                                    Valor = g.Count()
                                }).ToList();

        var defectosPorPrograma = defectos
                                    .GroupBy(x => x.Inspeccion.Transfer.Programa)
                                    .Select(g => new NombreValorDto()
                                    {
                                        Nombre = g.Key!,
                                        Valor = g.Count()
                                    }).ToList();

        var defectosPorLote = defectos
                                .GroupBy(x => new { x.Inspeccion.Transfer.Programa, x.Inspeccion.Transfer.Lote })
                                .Select(g => new NombreValorDto()
                                {
                                    Nombre = $"P:{g.First().Inspeccion.Transfer.Programa} - L:{g.First().Inspeccion.Transfer.Lote}",
                                    Valor = g.Count()
                                }).ToList();

        var planosTopPorOp = await defectos
                    .GroupBy(id => new
                    {
                        id.Inspeccion.IdOperacion,
                        Operacion = id.Inspeccion.Operacion.Nombre,
                        id.IdDefecto,
                        Defecto = id.Defecto.Nombre
                    })
                    .Select(g => new
                    {
                        g.Key.IdOperacion,
                        g.Key.Operacion,
                        g.Key.Defecto,
                        Valor = g.Count()
                    })
                    .ToListAsync();

        var topDefectosPorOperacion = planosTopPorOp
            .GroupBy(x => new { x.IdOperacion, x.Operacion })
            .Select(g => new TopDefectosPorOperacionDto
            {
                IdOperacion = g.Key.IdOperacion,
                Operacion = g.Key.Operacion,
                Defectos = g
                    .OrderByDescending(x => x.Valor)
                    .ThenBy(x => x.Defecto)
                    .Take(topPorOperacion)
                    .Select(x => new NombreValorDto { Nombre = x.Defecto, Valor = x.Valor })
                    .ToList()
            })
            .OrderBy(x => x.Operacion)
            .ToList();

        var planosHora = await defectos
                    .GroupBy(id => new
                    {
                        id.FechaRegistro.Year,
                        id.FechaRegistro.Month,
                        id.FechaRegistro.Day,
                        id.FechaRegistro.Hour
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        Valor = g.Count()
                    })
                    .ToListAsync();

        var defectosPorDiaHora = planosHora
            .GroupBy(x => new DateTime(x.Year, x.Month, x.Day))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var porHora = g.ToDictionary(x => x.Hour, x => x.Valor);
                var horas = Enumerable.Range(0, 24)
                    .Select(h => new NombreValorDto
                    {
                        Nombre = h.ToString("00"),
                        Valor = porHora.TryGetValue(h, out var v) ? v : 0
                    })
                    .ToList();

                return new DefectosPorDiaHoraDto
                {
                    Fecha = g.Key.ToString(FormatoFecha),
                    Horas = horas
                };
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
            DefectosPorTiempo = new SerieNombreValorDto
            {
                Nombre = SerieDefectosNombre,
                Series = serieTiempo
            },
            CriticidadDefectos = defectosCriticidad,
            DefectosPorLado = defectosPorLado,
            DefectosPorPrograma = defectosPorPrograma,
            DefectosPorLote = defectosPorLote,
            TopDefectosPorOperacion = topDefectosPorOperacion,
            DefectosPorDiaHora = defectosPorDiaHora
        };
    }

    public async Task<IReadOnlyList<DashboardDetalleItemDto>> GetDetalleAsync(DashboardDetalleFiltro f)
    {
        var desde = f.StartDate.Date;
        var hastaExcl = f.EndDate.Date.AddDays(1);

        var q = _context.InspeccionDetalles.AsNoTracking()
            .Include(d => d.Defecto).ThenInclude(def => def.Criticidad)
            .Include(d => d.Usuario)
            .Include(d => d.Inspeccion).ThenInclude(i => i.Transfer)
            .Include(d => d.Inspeccion).ThenInclude(i => i.Operacion)
            .Include(d => d.Inspeccion).ThenInclude(i => i.TipoInspeccion)
            .Where(d => d.FechaRegistro >= desde && d.FechaRegistro < hastaExcl);

        if (f.Fecha.HasValue)
        {
            var d0 = f.Fecha.Value.Date;
            var d1 = d0.AddDays(1);
            q = q.Where(d => d.FechaRegistro >= d0 && d.FechaRegistro < d1);
        }

        if (f.Hora.HasValue)
            q = q.Where(d => d.FechaRegistro.Hour == f.Hora.Value);

        if (f.IdOperacion.HasValue)
            q = q.Where(d => d.Inspeccion.IdOperacion == f.IdOperacion);

        if (f.IdDefecto.HasValue)
            q = q.Where(d => d.IdDefecto == f.IdDefecto);

        if (!string.IsNullOrWhiteSpace(f.Programa))
            q = q.Where(d => d.Inspeccion.Transfer.Programa == f.Programa);

        if (!string.IsNullOrWhiteSpace(f.Lote))
            q = q.Where(d => d.Inspeccion.Transfer.Lote == f.Lote);

        if (!string.IsNullOrWhiteSpace(f.Lado))
            q = q.Where(d => d.Lado == f.Lado.Trim().ToUpper());

        if (f.IdCriticidad.HasValue)
            q = q.Where(d => d.Defecto.IdCriticidad == f.IdCriticidad);

        return await q
            .OrderByDescending(d => d.FechaRegistro)
            .Take(500) // límite de seguridad
            .Select(d => new DashboardDetalleItemDto(
                d.IdDetalle,
                d.IdInspeccion,
                d.FechaRegistro,
                d.Inspeccion.Transfer.Programa,
                d.Inspeccion.Transfer.Lote,
                d.Inspeccion.Transfer.Lista,
                d.Inspeccion.Transfer.Punto,
                d.Inspeccion.Operacion.Nombre,
                d.Inspeccion.TipoInspeccion.Codigo,
                d.IdDefecto,
                d.Defecto.Codigo,
                d.Defecto.Nombre,
                d.Defecto.Criticidad != null ? d.Defecto.Criticidad.Nombre : null,
                d.TipoRegistro,
                d.Lado,
                d.Cantidad,
                d.Usuario.Nombre
            ))
            .ToListAsync();
    }
}

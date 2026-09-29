using ClosedXML.Excel;
using Calidad_API.Data;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class ReporteService : IReporteService
    {
        private readonly ApplicationDbContext _context;

        public ReporteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> DefectosDetalleExcelAsync(
            DateTime desde, DateTime hasta,
            long? idOperacion, string? programa, string? lote, string? tipoRegistro)
        {
            var query = _context.InspeccionDetalles.AsNoTracking()
                .Include(d => d.Defecto)
                .Include(d => d.Usuario)
                .Include(d => d.Inspeccion).ThenInclude(i => i.Transfer)
                .Include(d => d.Inspeccion).ThenInclude(i => i.Operacion)
                .Include(d => d.Inspeccion).ThenInclude(i => i.TipoInspeccion)
                .Where(d => d.FechaRegistro >= desde && d.FechaRegistro < hasta.AddDays(1));

            if (idOperacion.HasValue)
                query = query.Where(d => d.Inspeccion.IdOperacion == idOperacion);
            if (!string.IsNullOrWhiteSpace(programa))
                query = query.Where(d => d.Inspeccion.Transfer.Programa == programa);
            if (!string.IsNullOrWhiteSpace(lote))
                query = query.Where(d => d.Inspeccion.Transfer.Lote == lote);
            if (!string.IsNullOrWhiteSpace(tipoRegistro))
                query = query.Where(d => d.TipoRegistro == tipoRegistro.Trim().ToUpper());

            var rows = await query
                .OrderBy(d => d.FechaRegistro)
                .Select(d => new
                {
                    d.FechaRegistro,
                    Programa = d.Inspeccion.Transfer.Programa,
                    Lote = d.Inspeccion.Transfer.Lote,
                    Lista = d.Inspeccion.Transfer.Lista,
                    Punto = d.Inspeccion.Transfer.Punto,
                    Operacion = d.Inspeccion.Operacion.Nombre,
                    TipoInspeccion = d.Inspeccion.TipoInspeccion.Codigo,
                    DefectoCodigo = d.Defecto.Codigo,
                    DefectoNombre = d.Defecto.Nombre,
                    d.TipoRegistro,
                    d.Lado,
                    d.Cantidad,
                    Usuario = d.Usuario.Nombre
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Defectos detalle");

            var headers = new[]
            {
                "Fecha", "Programa", "Lote", "Lista", "Punto", "Operación",
                "Tipo inspección", "Cód. defecto", "Defecto", "Tipo registro",
                "Lado", "Cantidad", "Usuario"
            };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            var r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.FechaRegistro;
                ws.Cell(r, 2).Value = x.Programa;
                ws.Cell(r, 3).Value = x.Lote;
                ws.Cell(r, 4).Value = x.Lista;
                ws.Cell(r, 5).Value = x.Punto;
                ws.Cell(r, 6).Value = x.Operacion;
                ws.Cell(r, 7).Value = x.TipoInspeccion;
                ws.Cell(r, 8).Value = x.DefectoCodigo;
                ws.Cell(r, 9).Value = x.DefectoNombre;
                ws.Cell(r, 10).Value = x.TipoRegistro;
                ws.Cell(r, 11).Value = x.Lado;
                ws.Cell(r, 12).Value = x.Cantidad;
                ws.Cell(r, 13).Value = x.Usuario;
                r++;
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        public async Task<byte[]> DefectosResumenExcelAsync(
            DateTime desde, DateTime hasta, long? idOperacion, string? programa)
        {
            var query = _context.InspeccionDetalles.AsNoTracking()
                .Include(d => d.Defecto)
                .Include(d => d.Inspeccion).ThenInclude(i => i.Transfer)
                .Where(d => d.FechaRegistro >= desde && d.FechaRegistro < hasta.AddDays(1));

            if (idOperacion.HasValue)
                query = query.Where(d => d.Inspeccion.IdOperacion == idOperacion);
            if (!string.IsNullOrWhiteSpace(programa))
                query = query.Where(d => d.Inspeccion.Transfer.Programa == programa);

            var data = await query.ToListAsync();
            var total = data.Sum(x => x.Cantidad);

            var grupos = data
                .GroupBy(d => new { d.IdDefecto, d.Defecto.Codigo, d.Defecto.Nombre })
                .Select(g => new
                {
                    g.Key.Codigo,
                    g.Key.Nombre,
                    Piochas = g.Where(x => x.TipoRegistro == "PIOCHA").Sum(x => x.Cantidad),
                    Reprocesos = g.Where(x => x.TipoRegistro == "REPROCESO").Sum(x => x.Cantidad),
                    Total = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Resumen defectos");
            ws.Cell(1, 1).Value = "Código";
            ws.Cell(1, 2).Value = "Defecto";
            ws.Cell(1, 3).Value = "Piochas";
            ws.Cell(1, 4).Value = "Reprocesos";
            ws.Cell(1, 5).Value = "Total";
            ws.Cell(1, 6).Value = "% sobre total";
            ws.Range(1, 1, 1, 6).Style.Font.Bold = true;

            var r = 2;
            foreach (var g in grupos)
            {
                ws.Cell(r, 1).Value = g.Codigo;
                ws.Cell(r, 2).Value = g.Nombre;
                ws.Cell(r, 3).Value = g.Piochas;
                ws.Cell(r, 4).Value = g.Reprocesos;
                ws.Cell(r, 5).Value = g.Total;
                ws.Cell(r, 6).Value = total > 0 ? Math.Round(g.Total * 100m / total, 2) : 0;
                r++;
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        public async Task<byte[]> PorLoteExcelAsync(
            DateTime desde, DateTime hasta, string? programa, string? lote)
        {
            var query = _context.Inspecciones.AsNoTracking()
                .Include(i => i.Transfer)
                .Include(i => i.Detalles)
                .Where(i => i.FechaInspeccion >= desde && i.FechaInspeccion < hasta.AddDays(1));

            if (!string.IsNullOrWhiteSpace(programa))
                query = query.Where(i => i.Transfer.Programa == programa);
            if (!string.IsNullOrWhiteSpace(lote))
                query = query.Where(i => i.Transfer.Lote == lote);

            var list = await query.ToListAsync();

            var grupos = list
                .GroupBy(i => new { i.Transfer.Programa, i.Transfer.Lote })
                .Select(g => new
                {
                    g.Key.Programa,
                    g.Key.Lote,
                    Inspecciones = g.Count(),
                    Defectos = g.SelectMany(x => x.Detalles).Sum(d => d.Cantidad),
                    Piochas = g.SelectMany(x => x.Detalles).Where(d => d.TipoRegistro == "PIOCHA").Sum(d => d.Cantidad),
                    Reprocesos = g.SelectMany(x => x.Detalles).Where(d => d.TipoRegistro == "REPROCESO").Sum(d => d.Cantidad)
                })
                .OrderBy(x => x.Programa).ThenBy(x => x.Lote)
                .ToList();

            // Vales por lote
            var vales = await _context.Vales.AsNoTracking()
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .Where(v => v.FechaGeneracion >= desde && v.FechaGeneracion < hasta.AddDays(1))
                .ToListAsync();

            var valesPorLote = vales
                .GroupBy(v => new { v.Inspeccion.Transfer.Programa, v.Inspeccion.Transfer.Lote })
                .ToDictionary(g => (g.Key.Programa, g.Key.Lote), g => g.Count());

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Por lote");
            var headers = new[] { "Programa", "Lote", "Inspecciones", "Defectos", "Piochas", "Reprocesos", "Vales" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            var r = 2;
            foreach (var g in grupos)
            {
                ws.Cell(r, 1).Value = g.Programa;
                ws.Cell(r, 2).Value = g.Lote;
                ws.Cell(r, 3).Value = g.Inspecciones;
                ws.Cell(r, 4).Value = g.Defectos;
                ws.Cell(r, 5).Value = g.Piochas;
                ws.Cell(r, 6).Value = g.Reprocesos;
                valesPorLote.TryGetValue((g.Programa, g.Lote), out var nVales);
                ws.Cell(r, 7).Value = nVales;
                r++;
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        public async Task<byte[]> ValesExcelAsync(
            DateTime desde, DateTime hasta, string? estado, long? idOperacion)
        {
            var query = _context.Vales.AsNoTracking()
                .Include(v => v.UsuarioSolicita)
                .Include(v => v.Detalles).ThenInclude(d => d.Pieza)
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .Include(v => v.Inspeccion).ThenInclude(i => i.Operacion)
                .Where(v => v.FechaGeneracion >= desde && v.FechaGeneracion < hasta.AddDays(1));

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(v => v.Estado == estado.Trim().ToUpper());
            if (idOperacion.HasValue)
                query = query.Where(v => v.Inspeccion.IdOperacion == idOperacion);

            var list = await query.OrderByDescending(v => v.FechaGeneracion).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Vales");
            var headers = new[]
            {
                "Folio", "Fecha", "Estado", "Programa", "Lote", "Operación",
                "Solicita", "Cód. pieza", "Pieza", "Cantidad"
            };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            var r = 2;
            foreach (var v in list)
            {
                if (v.Detalles.Count == 0)
                {
                    ws.Cell(r, 1).Value = v.Folio;
                    ws.Cell(r, 2).Value = v.FechaGeneracion;
                    ws.Cell(r, 3).Value = v.Estado;
                    ws.Cell(r, 4).Value = v.Inspeccion.Transfer.Programa;
                    ws.Cell(r, 5).Value = v.Inspeccion.Transfer.Lote;
                    ws.Cell(r, 6).Value = v.Inspeccion.Operacion.Nombre;
                    ws.Cell(r, 7).Value = v.UsuarioSolicita.Nombre;
                    r++;
                    continue;
                }

                foreach (var d in v.Detalles)
                {
                    ws.Cell(r, 1).Value = v.Folio;
                    ws.Cell(r, 2).Value = v.FechaGeneracion;
                    ws.Cell(r, 3).Value = v.Estado;
                    ws.Cell(r, 4).Value = v.Inspeccion.Transfer.Programa;
                    ws.Cell(r, 5).Value = v.Inspeccion.Transfer.Lote;
                    ws.Cell(r, 6).Value = v.Inspeccion.Operacion.Nombre;
                    ws.Cell(r, 7).Value = v.UsuarioSolicita.Nombre;
                    ws.Cell(r, 8).Value = d.Pieza.Codigo;
                    ws.Cell(r, 9).Value = d.Pieza.Nombre;
                    ws.Cell(r, 10).Value = d.Cantidad;
                    r++;
                }
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        public async Task<byte[]> IndicadoresExcelAsync(
            DateTime desde, DateTime hasta, long? idOperacion, string? programa)
        {
            var inspQuery = _context.Inspecciones.AsNoTracking()
                .Include(i => i.Transfer)
                .Include(i => i.Detalles)
                .Where(i => i.FechaInspeccion >= desde && i.FechaInspeccion < hasta.AddDays(1));

            if (idOperacion.HasValue)
                inspQuery = inspQuery.Where(i => i.IdOperacion == idOperacion);
            if (!string.IsNullOrWhiteSpace(programa))
                inspQuery = inspQuery.Where(i => i.Transfer.Programa == programa);

            var inspecciones = await inspQuery.ToListAsync();
            var nInsp = inspecciones.Count;
            var nDef = inspecciones.SelectMany(i => i.Detalles).Sum(d => d.Cantidad);
            var nPiocha = inspecciones.SelectMany(i => i.Detalles)
                .Where(d => d.TipoRegistro == "PIOCHA").Sum(d => d.Cantidad);
            var nRepr = inspecciones.SelectMany(i => i.Detalles)
                .Where(d => d.TipoRegistro == "REPROCESO").Sum(d => d.Cantidad);

            var valesQuery = _context.Vales.AsNoTracking()
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .Where(v => v.FechaGeneracion >= desde && v.FechaGeneracion < hasta.AddDays(1));
            if (idOperacion.HasValue)
                valesQuery = valesQuery.Where(v => v.Inspeccion.IdOperacion == idOperacion);
            if (!string.IsNullOrWhiteSpace(programa))
                valesQuery = valesQuery.Where(v => v.Inspeccion.Transfer.Programa == programa);
            var nVales = await valesQuery.CountAsync();

            // Producción (si hay datos en produccion_diaria)
            decimal? paresProd = null;
            if (idOperacion.HasValue)
            {
                var fechaDesde = DateOnly.FromDateTime(desde);
var fechaHasta = DateOnly.FromDateTime(hasta);
                paresProd = await _context.ProduccionesDiarias.AsNoTracking()
                    .Where(p => p.IdOperacion == idOperacion
                                && p.Fecha == desde.Date
                                && p.Fecha == hasta.Date.AddDays(1))
                    .SumAsync(p => (decimal?)p.CantidadProducida); // ajusta nombre de columna real
            }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Indicadores");
            ws.Cell(1, 1).Value = "Indicador";
            ws.Cell(1, 2).Value = "Valor";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;

            ws.Cell(2, 1).Value = "Desde";
            ws.Cell(2, 2).Value = desde.ToString("yyyy-MM-dd");
            ws.Cell(3, 1).Value = "Hasta";
            ws.Cell(3, 2).Value = hasta.ToString("yyyy-MM-dd");
            ws.Cell(4, 1).Value = "Inspecciones";
            ws.Cell(4, 2).Value = nInsp;
            ws.Cell(5, 1).Value = "Total defectos (cantidad)";
            ws.Cell(5, 2).Value = nDef;
            ws.Cell(6, 1).Value = "Piochas";
            ws.Cell(6, 2).Value = nPiocha;
            ws.Cell(7, 1).Value = "Reprocesos";
            ws.Cell(7, 2).Value = nRepr;
            ws.Cell(8, 1).Value = "Vales generados";
            ws.Cell(8, 2).Value = nVales;
            ws.Cell(9, 1).Value = "Defectos por inspección";
            ws.Cell(9, 2).Value = nInsp > 0 ? Math.Round(nDef / (decimal)nInsp, 2) : 0;

            if (paresProd.HasValue && paresProd > 0)
            {
                ws.Cell(10, 1).Value = "Pares producidos";
                ws.Cell(10, 2).Value = paresProd.Value;
                ws.Cell(11, 1).Value = "PPM (piochas)";
                ws.Cell(11, 2).Value = Math.Round(nPiocha * 1_000_000m / paresProd.Value, 2);
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        public async Task<byte[]> InspeccionesExcelAsync(
            DateTime desde, DateTime hasta, long? idOperacion, string? lote)
        {
            var query = _context.Inspecciones.AsNoTracking()
                .Include(i => i.Transfer)
                .Include(i => i.Operacion)
                .Include(i => i.TipoInspeccion)
                .Include(i => i.Usuario)
                .Include(i => i.Detalles)
                .Where(i => i.FechaInspeccion >= desde && i.FechaInspeccion < hasta.AddDays(1));

            if (idOperacion.HasValue)
                query = query.Where(i => i.IdOperacion == idOperacion);
            if (!string.IsNullOrWhiteSpace(lote))
                query = query.Where(i => i.Transfer.Lote == lote);

            var list = await query.OrderByDescending(i => i.FechaInspeccion).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Inspecciones");
            var headers = new[]
            {
                "Id", "Fecha", "Programa", "Lote", "Operación", "Tipo",
                "Usuario", "Estado", "Dispositivo", "# Defectos", "Observaciones"
            };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            var r = 2;
            foreach (var i in list)
            {
                ws.Cell(r, 1).Value = i.IdInspeccion;
                ws.Cell(r, 2).Value = i.FechaInspeccion;
                ws.Cell(r, 3).Value = i.Transfer.Programa;
                ws.Cell(r, 4).Value = i.Transfer.Lote;
                ws.Cell(r, 5).Value = i.Operacion.Nombre;
                ws.Cell(r, 6).Value = i.TipoInspeccion.Codigo;
                ws.Cell(r, 7).Value = i.Usuario.Nombre;
                ws.Cell(r, 8).Value = i.Estado;
                ws.Cell(r, 9).Value = i.Dispositivo;
                ws.Cell(r, 10).Value = i.Detalles.Count;
                ws.Cell(r, 11).Value = i.Observaciones;
                r++;
            }

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

        private static byte[] ToBytes(XLWorkbook wb)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
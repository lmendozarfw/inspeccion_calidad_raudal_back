using Calidad_API.Data;
using Calidad_API.DTOs.Vales;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Calidad_API.Services
{
    public class ValeService : IValeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ValePdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ValeService(
            ApplicationDbContext context,
            ValePdfService pdfService,
            IEmailService emailService,
            IConfiguration config)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
            _config = config;
        }

        public async Task<ValeDto?> GetByIdAsync(long id)
        {
            var v = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.IdVale == id);

            return v is null ? null : Map(v);
        }

        public async Task<ValeDto?> GetByFolioAsync(string folio)
        {
            var v = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.Folio == folio);

            return v is null ? null : Map(v);
        }

        public async Task<IEnumerable<ValeDto>> GetByInspeccionDetalleAsync(long idInspeccion)
        {
            var list = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .Where(x => x.IdInspeccion == idInspeccion)
                .OrderByDescending(x => x.FechaGeneracion)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<ValeDto> GenerarAsync(ValeCreateDto dto, long idUsuario)
        {
            var inspeccion = await _context.Inspecciones
                .Include(i => i.Transfer)
                .Include(i => i.Detalles)
                    .ThenInclude(d => d.Defecto)
                .FirstOrDefaultAsync(i => i.IdInspeccion == dto.IdInspeccion);

            if (inspeccion is null)
                throw new InvalidOperationException("La inspección no existe.");

            var puedeGenerar = await _context.PermisosOperacion.AnyAsync(p =>
                p.IdUsuario == idUsuario
                && p.IdOperacion == inspeccion.IdOperacion
                && p.PuedeGenerarVale);

            if (!puedeGenerar)
                throw new UnauthorizedAccessException("No tienes permiso para generar vales en esta operación.");

            // 1) Líneas: las del body, o automáticas desde detalles con pieza
            List<(long IdPieza, decimal Cantidad)> lineasMerged;

            if (dto.Lineas is { Count: > 0 })
            {
                lineasMerged = dto.Lineas
                    .GroupBy(x => x.IdPieza)
                    .Select(g => (g.Key, g.Sum(x => x.Cantidad)))
                    .ToList();
            }
            else
            {
                // Automático: defectos que requieren pieza
                lineasMerged = inspeccion.Detalles
                    .Where(d => d.Defecto.AplicaPieza
                                && d.Defecto.IdPieza.HasValue
                                && d.Defecto.IdPieza.Value > 0)
                    .GroupBy(d => d.Defecto.IdPieza!.Value)
                    .Select(g => (IdPieza: g.Key, Cantidad: g.Sum(x => x.Cantidad)))
                    .ToList();
            }

            if (lineasMerged.Count == 0)
                throw new InvalidOperationException(
                    "No hay materiales para el vale. Los defectos de la inspección no tienen pieza asociada, " +
                    "o indica 'lineas' manualmente.");

            if (lineasMerged.Any(x => x.Cantidad <= 0))
                throw new InvalidOperationException("La cantidad de cada línea debe ser mayor a cero.");

            var idsPieza = lineasMerged.Select(x => x.IdPieza).ToList();
            var piezas = await _context.Piezas
                .Where(p => idsPieza.Contains(p.IdPieza) && p.Activo)
                .ToListAsync();

            if (piezas.Count != idsPieza.Count)
                throw new InvalidOperationException("Una o más piezas no existen o están inactivas.");

            var folio = await ObtenerSiguienteFolioAsync();

            var vale = new Vale
            {
                Folio = folio,
                IdInspeccion = dto.IdInspeccion,
                IdUsuarioSolicita = idUsuario,
                FechaGeneracion = DateTime.UtcNow,
                Estado = "GENERADO"
            };

            foreach (var linea in lineasMerged)
            {
                vale.Detalles.Add(new ValeDetalle
                {
                    IdPieza = linea.IdPieza,
                    Cantidad = linea.Cantidad
                });
            }

            _context.Vales.Add(vale);
            await _context.SaveChangesAsync();

            // Recargar + PDF + correo (igual que ya tienes)
            vale = await _context.Vales
                .Include(v => v.UsuarioSolicita)
                .Include(v => v.Detalles).ThenInclude(d => d.Pieza)
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .FirstAsync(v => v.IdVale == vale.IdVale);

            var inspeccionFull = await _context.Inspecciones
                .AsNoTracking()
                .Include(i => i.Transfer)
                .Include(i => i.Detalles).ThenInclude(d => d.Defecto)
                .FirstAsync(i => i.IdInspeccion == vale.IdInspeccion);

            var lote = inspeccionFull.Transfer.Lote;

            try
            {
                vale.RutaPdf = _pdfService.GenerarYGuardar(vale, inspeccionFull, lote);
                await _context.SaveChangesAsync();
            }
            catch { /* log */ }

            // correo...
            try
            {
                var destinatarios = (_config["Email:AvisoValeTo"] ?? "")
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (destinatarios.Length > 0)
                {
                    var subject = $"Vale de material generado — {vale.Folio}";
                    var body = $@"
            <p>Se generó el vale de material <b>{vale.Folio}</b>.</p>
            <p>
              Lote: <b>{lote}</b><br/>
              Solicita: <b>{vale.UsuarioSolicita.Nombre}</b><br/>
              Fecha: <b>{vale.FechaGeneracion:dd/MM/yyyy HH:mm} UTC</b>
            </p>
            <p>Se adjunta el PDF del vale para su revisión e impresión.</p>
            <p style='color:#666;font-size:12px;'>Módulo de Calidad — aviso automático</p>";

                    await _emailService.SendAsync(
                        destinatarios,
                        subject,
                        body,
                        vale.RutaPdf,           // ruta del archivo
                        $"{vale.Folio}.pdf");   // nombre del adjunto
                }
            }
            catch
            {
                // No tumbar la generación del vale si falla el correo
            }

            return Map(vale);
        }

        public async Task<ValeDto?> ActualizarEstadoAsync(long id, ValeUpdateEstadoDto dto)
        {
            var vale = await _context.Vales
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.IdVale == id);

            if (vale is null) return null;

            var estado = dto.Estado.Trim().ToUpper();
            if (estado is not ("GENERADO" or "ENTREGADO" or "CANCELADO"))
                throw new InvalidOperationException("Estado inválido. Use GENERADO, ENTREGADO o CANCELADO.");

            vale.Estado = estado;
            await _context.SaveChangesAsync();

            return Map(vale);
        }

        /// <summary>
        /// Llama al procedimiento almacenado usp_ObtenerSiguienteFolioVale
        /// </summary>
        private async Task<string> ObtenerSiguienteFolioAsync()
        {
            var folioParam = new SqlParameter
            {
                ParameterName = "@Folio",
                SqlDbType = SqlDbType.VarChar,
                Size = 20,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_ObtenerSiguienteFolioVale @Folio OUTPUT",
                folioParam);

            var folio = folioParam.Value?.ToString();
            if (string.IsNullOrWhiteSpace(folio))
                throw new InvalidOperationException("No se pudo obtener el siguiente folio de vale.");

            return folio;
        }

        private static ValeDto Map(Vale v) => new(
    v.IdVale,
    v.Folio,
    v.IdInspeccion,
    v.IdUsuarioSolicita,
    v.UsuarioSolicita.Nombre,
    v.FechaGeneracion,
    v.RutaPdf,
    v.Estado,
    v.Detalles
        .OrderBy(d => d.Pieza.Codigo)
        .Select(d => new ValeLineaDto(
            d.IdValeDetalle,
            d.IdPieza,
            d.Pieza.Codigo,
            d.Pieza.Nombre,
            d.Cantidad))
        .ToList()
);

        public async Task<IEnumerable<ValeDto>> GetByInspeccionAsync(long idInspeccion)
        {
            var list = await _context.Vales
                .AsNoTracking()
                .Include(x => x.UsuarioSolicita)
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Where(x => x.IdInspeccion == idInspeccion)
                .OrderByDescending(x => x.FechaGeneracion)
                .ToListAsync();

            return list.Select(Map);
        }
    }
}
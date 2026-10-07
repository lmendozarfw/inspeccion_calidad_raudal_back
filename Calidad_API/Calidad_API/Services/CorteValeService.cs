using Calidad_API.Data;
using Calidad_API.DTOs.Vales;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class CorteValeService : ICorteValeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<CorteValeService> _logger;
        private readonly ICodigoAutorizacionService _codigoAuth;
        private readonly ValePdfService _pdfService;

        public CorteValeService(
            ApplicationDbContext context,
            IEmailService email,
            IConfiguration config,
            ILogger<CorteValeService> logger)
        {
            _context = context;
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task<CorteEjecutarResponse> EjecutarCorteAsync()
        {
            var pendientes = await _context.SolicitudesMaterial
                .Include(s => s.Pieza)
                .Include(s => s.Operacion)
                .Where(s => s.Estado == "PENDIENTE")
                .OrderBy(s => s.FechaSolicitud)
                .ToListAsync();

            if (pendientes.Count == 0)
                return new CorteEjecutarResponse(0, Array.Empty<CorteResumenDto>());

            var grupos = pendientes.GroupBy(s => s.IdOperacion).ToList();
            var creados = new List<CorteResumenDto>();

            foreach (var g in grupos)
            {
                var op = g.First().Operacion;
                var corte = new CorteVale
                {
                    IdOperacion = g.Key,
                    FechaCorte = DateTime.UtcNow,
                    Estado = "PENDIENTE_AUTH"
                };

                _context.CortesVale.Add(corte);
                await _context.SaveChangesAsync(); // para tener IdCorte

                foreach (var sol in g)
                {
                    sol.Estado = "EN_CORTE";
                    sol.IdCorte = corte.IdCorte;
                }

                await _context.SaveChangesAsync();

                var resumen = new CorteResumenDto(
                    corte.IdCorte,
                    op.IdOperacion,
                    op.Codigo,
                    op.Nombre,
                    corte.FechaCorte,
                    corte.Estado,
                    g.Count(),
                    g.Sum(x => x.Cantidad));

                creados.Add(resumen);

                try
                {
                    await NotificarSupervisorAsync(resumen, g.ToList());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al notificar corte {IdCorte}", corte.IdCorte);
                }
            }

            return new CorteEjecutarResponse(creados.Count, creados);
        }

        public async Task<IEnumerable<CorteResumenDto>> GetPendientesAsync()
        {
            var list = await _context.CortesVale.AsNoTracking()
                .Include(c => c.Operacion)
                .Include(c => c.Solicitudes)
                .Where(c => c.Estado == "PENDIENTE_AUTH")
                .OrderByDescending(c => c.FechaCorte)
                .ToListAsync();

            return list.Select(c => new CorteResumenDto(
                c.IdCorte,
                c.IdOperacion,
                c.Operacion.Codigo,
                c.Operacion.Nombre,
                c.FechaCorte,
                c.Estado,
                c.Solicitudes.Count,
                c.Solicitudes.Sum(s => s.Cantidad)));
        }

        public async Task<CorteDetalleDto?> GetByIdAsync(long idCorte)
        {
            var c = await _context.CortesVale.AsNoTracking()
                .Include(x => x.Operacion)
                .Include(x => x.Solicitudes).ThenInclude(s => s.Pieza)
                .FirstOrDefaultAsync(x => x.IdCorte == idCorte);

            if (c is null) return null;

            return new CorteDetalleDto(
                c.IdCorte,
                c.IdOperacion,
                c.Operacion.Codigo,
                c.Operacion.Nombre,
                c.FechaCorte,
                c.Estado,
                c.IdUsuarioAutoriza,
                c.FechaAutorizacion,
                c.IdVale,
                c.Solicitudes
                    .OrderBy(s => s.Lote)
                    .ThenBy(s => s.Pieza.Codigo)
                    .Select(s => new CorteLineaDto(
                        s.IdSolicitud,
                        s.Programa,
                        s.Lote,
                        s.Lado,
                        s.IdPieza,
                        s.Pieza.Codigo,
                        s.Pieza.Nombre,
                        s.Cantidad,
                        s.FechaSolicitud))
                    .ToList());
        }

        private async Task NotificarSupervisorAsync(
            CorteResumenDto resumen,
            List<SolicitudMaterial> lineas)
        {
            var destinatarios = await ObtenerEmailsSupervisoresAsync();
            if (destinatarios.Count == 0)
            {
                _logger.LogWarning("Corte {Id}: no hay correos de supervisor configurados.", resumen.IdCorte);
                return;
            }

            var filas = string.Join("", lineas.Select(s =>
                $"<tr><td>{s.Programa}</td><td>{s.Lote}</td><td>{s.Lado}</td>" +
                $"<td>{s.Pieza.Codigo}</td><td>{s.Pieza.Nombre}</td><td>{s.Cantidad:0.##}</td></tr>"));

            var subject = $"Corte de vale pendiente de autorización — {resumen.OperacionCodigo} #{resumen.IdCorte}";
            var body = $@"
                <h3>Corte de material por autorizar</h3>
                <ul>
                  <li><b>Corte:</b> {resumen.IdCorte}</li>
                  <li><b>Operación:</b> {resumen.OperacionCodigo} - {resumen.OperacionNombre}</li>
                  <li><b>Fecha (UTC):</b> {resumen.FechaCorte:dd/MM/yyyy HH:mm}</li>
                  <li><b>Solicitudes:</b> {resumen.NumSolicitudes}</li>
                  <li><b>Cantidad total:</b> {resumen.CantidadTotal:0.##}</li>
                </ul>
                <p>Revise y autorice con su código de supervisor.</p>
                <table border='1' cellpadding='4' cellspacing='0'>
                  <thead>
                    <tr>
                      <th>Programa</th><th>Lote</th><th>Lado</th>
                      <th>Cód. pieza</th><th>Pieza</th><th>Cant.</th>
                    </tr>
                  </thead>
                  <tbody>{filas}</tbody>
                </table>
                <p style='color:#666;font-size:12px;'>Módulo Calidad — aviso automático de corte</p>";

            await _email.SendAsync(destinatarios, subject, body);
        }

        private async Task<List<string>> ObtenerEmailsSupervisoresAsync()
        {
            var cfg = await _context.ConfigCortesVale.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (!string.IsNullOrWhiteSpace(cfg?.EmailSupervisores))
            {
                return cfg.EmailSupervisores
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }

            var fromConfig = (_config["Email:AvisoCorteTo"] ?? "")
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            return fromConfig;
        }

        public async Task<ValeDto> AutorizarCorteAsync(
    long idCorte,
    string codigoAutorizacion,
    long idUsuarioAutoriza)
        {
            if (string.IsNullOrWhiteSpace(codigoAutorizacion))
                throw new InvalidOperationException("El código de autorización es obligatorio.");

            var idSupervisor = await _codigoAuth.ValidarCodigoAsync(codigoAutorizacion);
            if (idSupervisor is null)
                throw new UnauthorizedAccessException("Código de autorización inválido, inactivo o vencido.");

            var corte = await _context.CortesVale
                .Include(c => c.Operacion)
                .Include(c => c.Solicitudes).ThenInclude(s => s.Pieza)
                .Include(c => c.Solicitudes).ThenInclude(s => s.Inspeccion).ThenInclude(i => i.Transfer)
                .FirstOrDefaultAsync(c => c.IdCorte == idCorte);

            if (corte is null)
                throw new InvalidOperationException("El corte no existe.");
            if (corte.Estado != "PENDIENTE_AUTH")
                throw new InvalidOperationException($"El corte no está pendiente de autorización (estado: {corte.Estado}).");
            if (corte.Solicitudes.Count == 0)
                throw new InvalidOperationException("El corte no tiene solicitudes.");

            // Folio
            var folio = await ObtenerSiguienteFolioAsync(); // copia el método del ValeService o extráelo a un helper

            var vale = new Vale
            {
                Folio = folio,
                // Si el vale exige id_inspeccion NOT NULL, usa la primera solicitud como referencia
                IdInspeccion = corte.Solicitudes.First().IdInspeccion,
                IdUsuarioSolicita = idUsuarioAutoriza, // o el que prefieran (quien autoriza)
                FechaGeneracion = DateTime.UtcNow,
                Estado = "GENERADO"
            };

            foreach (var sol in corte.Solicitudes.OrderBy(s => s.Lote).ThenBy(s => s.Pieza.Codigo))
            {
                vale.Detalles.Add(new ValeDetalle
                {
                    IdPieza = sol.IdPieza,
                    Cantidad = sol.Cantidad,
                    Programa = sol.Programa,
                    Lote = sol.Lote,
                    Lado = sol.Lado,
                    IdSolicitud = sol.IdSolicitud
                });
            }

            _context.Vales.Add(vale);
            await _context.SaveChangesAsync();

            // Marcar corte y solicitudes
            corte.Estado = "AUTORIZADO";
            corte.IdUsuarioAutoriza = idSupervisor.Value;
            corte.FechaAutorizacion = DateTime.UtcNow;
            corte.IdVale = vale.IdVale;

            foreach (var sol in corte.Solicitudes)
            {
                sol.Estado = "INCLUIDA_EN_VALE";
                sol.IdVale = vale.IdVale;
            }

            // Auditoría (si la usas)
            _context.RegistrosAutorizacion.Add(new RegistroAutorizacion
            {
                IdUsuarioSolicita = idUsuarioAutoriza,
                IdUsuarioAutoriza = idSupervisor.Value,
                IdInspeccion = vale.IdInspeccion,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Recargar navegaciones
            vale = await _context.Vales
                .Include(v => v.UsuarioSolicita)
                .Include(v => v.Detalles).ThenInclude(d => d.Pieza)
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .FirstAsync(v => v.IdVale == vale.IdVale);

            // PDF (firma puede necesitar adaptarse a varias líneas con traza)
            try
            {
                var loteRef = vale.Inspeccion.Transfer.Lote;
                var inspeccionFull = await _context.Inspecciones.AsNoTracking()
                    .Include(i => i.Transfer)
                    .Include(i => i.Detalles).ThenInclude(d => d.Defecto)
                    .FirstAsync(i => i.IdInspeccion == vale.IdInspeccion);

                vale.RutaPdf = _pdfService.GenerarYGuardar(vale, inspeccionFull, loteRef);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error PDF vale corte {Folio}", vale.Folio);
            }

            // Correo almacén (AvisoValeTo) + PDF si tienes overload con adjunto
            try
            {
                var to = (_config["Email:AvisoValeTo"] ?? "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (to.Length > 0)
                {
                    var filas = string.Join("", vale.Detalles.Select(d =>
                        $"<li>{d.Programa} | {d.Lote} | {d.Lado} | {d.Pieza.Codigo} - {d.Pieza.Nombre}: {d.Cantidad:0.##}</li>"));

                    var body = $@"
                <p>Se autorizó el corte y se generó el vale <b>{vale.Folio}</b>.</p>
                <p>Operación: {corte.Operacion.Codigo} - {corte.Operacion.Nombre}</p>
                <ul>{filas}</ul>
                <p>Se adjunta el PDF.</p>";

                    await _email.SendAsync(to, $"Vale generado (corte) — {vale.Folio}", body, vale.RutaPdf, $"{vale.Folio}.pdf");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error correo vale corte {Folio}", vale.Folio);
            }

            return MapVale(vale);
        }

        private static ValeDto MapVale(Vale v) => new(
            v.IdVale,
            v.Folio,
            v.IdInspeccion,
            v.IdUsuarioSolicita,
            v.UsuarioSolicita.Nombre,
            v.FechaGeneracion,
            v.RutaPdf,
            v.Estado,
            v.Detalles
                .OrderBy(d => d.Lote)
                .ThenBy(d => d.Pieza.Codigo)
                .Select(d => new ValeLineaDto(
                    d.IdValeDetalle,
                    d.IdPieza,
                    d.Pieza.Codigo,
                    d.Pieza.Nombre,
                    d.Cantidad))
                .ToList()
        );

        // Copia de ValeService:
        private async Task<string> ObtenerSiguienteFolioAsync()
        {
            var folioParam = new Microsoft.Data.SqlClient.SqlParameter
            {
                ParameterName = "@Folio",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Size = 20,
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_ObtenerSiguienteFolioVale @Folio OUTPUT",
                folioParam);

            var folio = folioParam.Value?.ToString();
            if (string.IsNullOrWhiteSpace(folio))
                throw new InvalidOperationException("No se pudo obtener el siguiente folio de vale.");
            return folio;
        }
    }
}
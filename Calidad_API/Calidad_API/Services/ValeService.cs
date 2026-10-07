using Calidad_API.Data;
using Calidad_API.DTOs.Vales;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Calidad_API.Services
{
    /// <summary>
    /// Servicio para manejar la generación, consulta y actualización de vales de material.
    /// </summary>
    public class ValeService : IValeService
    {
        /// <summary>
        /// Contexto de la base de datos para acceder a los vales, inspecciones y piezas.
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Servicio para generar y guardar archivos PDF de los vales.
        /// </summary>
        private readonly ValePdfService _pdfService;

        /// <summary>
        /// Servicio para enviar correos electrónicos, utilizado para notificar sobre la generación de vales.
        /// </summary>
        private readonly IEmailService _emailService;

        /// <summary>
        /// Configuración de la aplicación, utilizada para obtener parámetros como destinatarios de correo.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Servicio para validar códigos de autorización de supervisores, utilizado para verificar permisos al generar vales.
        /// </summary>
        private readonly ICodigoAutorizacionService _codigoAuth;

        /// <summary>
        /// Constructor del servicio de vales, inyectando dependencias necesarias para la operación.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pdfService"></param>
        /// <param name="emailService"></param>
        /// <param name="config"></param>
        public ValeService(
            ApplicationDbContext context,
            ValePdfService pdfService,
            IEmailService emailService,
            IConfiguration config,
            ICodigoAutorizacionService codigoAuth)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
            _config = config;
            _codigoAuth = codigoAuth;
        }

        /// <summary>
        /// Obtiene un vale por su ID, incluyendo detalles y usuario solicitante.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ValeDto?> GetByIdAsync(long id)
        {
            var v = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.IdVale == id);

            return v is null ? null : Map(v);
        }

        /// <summary>
        /// Obtiene un vale por su folio, incluyendo detalles y usuario solicitante.
        /// </summary>
        /// <param name="folio"></param>
        /// <returns></returns>
        public async Task<ValeDto?> GetByFolioAsync(string folio)
        {
            var v = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Detalles).ThenInclude(d => d.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.Folio == folio);

            return v is null ? null : Map(v);
        }

        /// <summary>
        /// Obtiene todos los vales asociados a una inspección específica, incluyendo detalles y usuario solicitante.
        /// </summary>
        /// <param name="idInspeccion"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Genera un nuevo vale de material basado en una inspección existente, verificando permisos y creando líneas de vale según los defectos asociados.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<ValeDto> GenerarAsync(ValeCreateDto dto, long idUsuario)
        {
            if (string.IsNullOrWhiteSpace(dto.CodigoAutorizacion))
                throw new InvalidOperationException("El código de autorización es obligatorio.");

            var idSupervisor = await _codigoAuth.ValidarCodigoAsync(dto.CodigoAutorizacion);
            if (idSupervisor is null)
                throw new UnauthorizedAccessException(
                    "Código de autorización inválido, inactivo o vencido.");

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

            // —— Líneas del vale (urgente) ——
            // Preferencia: solicitudes PENDIENTE de esta inspección
            // Si no hay: body Lineas o defectos con pieza (como antes)

            var solsPend = await _context.SolicitudesMaterial
                .Include(s => s.Pieza)
                .Where(s => s.IdInspeccion == dto.IdInspeccion && s.Estado == "PENDIENTE")
                .ToListAsync();

            var folio = await ObtenerSiguienteFolioAsync();
            var transfer = inspeccion.Transfer;

            var vale = new Vale
            {
                Folio = folio,
                IdInspeccion = dto.IdInspeccion,
                IdUsuarioSolicita = idUsuario,
                FechaGeneracion = DateTime.UtcNow,
                Estado = "GENERADO"
            };

            if (dto.Lineas is { Count: > 0 })
            {
                // Manual desde el body
                var lineasMerged = dto.Lineas
                    .GroupBy(x => x.IdPieza)
                    .Select(g => (IdPieza: g.Key, Cantidad: g.Sum(x => x.Cantidad)))
                    .ToList();

                if (lineasMerged.Any(x => x.Cantidad <= 0))
                    throw new InvalidOperationException("La cantidad de cada línea debe ser mayor a cero.");

                var idsPieza = lineasMerged.Select(x => x.IdPieza).ToList();
                var piezasOk = await _context.Piezas
                    .Where(p => idsPieza.Contains(p.IdPieza) && p.Activo)
                    .CountAsync();
                if (piezasOk != idsPieza.Count)
                    throw new InvalidOperationException("Una o más piezas no existen o están inactivas.");

                foreach (var linea in lineasMerged)
                {
                    vale.Detalles.Add(new ValeDetalle
                    {
                        IdPieza = linea.IdPieza,
                        Cantidad = linea.Cantidad,
                        Programa = transfer.Programa,
                        Lote = transfer.Lote,
                        Lado = null
                    });
                }
            }
            else if (solsPend.Count > 0)
            {
                // Desde cola de solicitudes (urgente)
                foreach (var sol in solsPend.OrderBy(s => s.Lote).ThenBy(s => s.Pieza.Codigo))
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
            }
            else
            {
                // Fallback: defectos de la inspección con pieza
                var lineasMerged = inspeccion.Detalles
                    .Where(d => d.Defecto.AplicaPieza
                                && d.Defecto.IdPieza.HasValue
                                && d.Defecto.IdPieza.Value > 0)
                    .GroupBy(d => d.Defecto.IdPieza!.Value)
                    .Select(g => (IdPieza: g.Key, Cantidad: g.Sum(x => x.Cantidad)))
                    .ToList();

                if (lineasMerged.Count == 0)
                    throw new InvalidOperationException(
                        "No hay materiales para el vale. No hay solicitudes pendientes ni defectos con pieza.");

                if (lineasMerged.Any(x => x.Cantidad <= 0))
                    throw new InvalidOperationException("La cantidad de cada línea debe ser mayor a cero.");

                foreach (var linea in lineasMerged)
                {
                    vale.Detalles.Add(new ValeDetalle
                    {
                        IdPieza = linea.IdPieza,
                        Cantidad = linea.Cantidad,
                        Programa = transfer.Programa,
                        Lote = transfer.Lote,
                        Lado = null
                    });
                }
            }

            if (vale.Detalles.Count == 0)
                throw new InvalidOperationException("No hay líneas de material para el vale.");

            _context.Vales.Add(vale);

            _context.RegistrosAutorizacion.Add(new RegistroAutorizacion
            {
                IdUsuarioSolicita = idUsuario,
                IdUsuarioAutoriza = idSupervisor.Value,
                IdInspeccion = dto.IdInspeccion,
                FechaCreacion = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // ★ Marcar solicitudes para que no entren al próximo corte
            var solsAMarcar = await _context.SolicitudesMaterial
                .Where(s => s.IdInspeccion == dto.IdInspeccion
                            && (s.Estado == "PENDIENTE" || s.Estado == "EN_CORTE"))
                .ToListAsync();

            foreach (var s in solsAMarcar)
            {
                s.Estado = "INCLUIDA_EN_VALE";
                s.IdVale = vale.IdVale;
            }

            if (solsAMarcar.Count > 0)
                await _context.SaveChangesAsync();

            // —— A partir de aquí dejas IGUAL lo que ya tienes: ——
            // recargar vale, PDF, correo, return Map(vale)
            vale = await _context.Vales
                .Include(v => v.UsuarioSolicita)
                .Include(v => v.Detalles).ThenInclude(d => d.Pieza)
                .Include(v => v.Inspeccion).ThenInclude(i => i.Transfer)
                .FirstAsync(v => v.IdVale == vale.IdVale);

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

        /// <summary>
        /// Actualiza el estado de un vale existente, permitiendo cambiarlo a "GENERADO", "ENTREGADO" o "CANCELADO".
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Mapea un objeto Vale a un ValeDto, incluyendo detalles y nombre del usuario solicitante.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene todos los vales asociados a una inspección específica, incluyendo detalles y usuario solicitante, ordenados por fecha de generación descendente.
        /// </summary>
        /// <param name="idInspeccion"></param>
        /// <returns></returns>
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
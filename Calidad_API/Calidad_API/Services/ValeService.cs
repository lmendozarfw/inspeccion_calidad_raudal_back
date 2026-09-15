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
                .Include(x => x.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.IdVale == id);

            return v is null ? null : Map(v);
        }

        public async Task<ValeDto?> GetByFolioAsync(string folio)
        {
            var v = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Pieza)
                .Include(x => x.UsuarioSolicita)
                .FirstOrDefaultAsync(x => x.Folio == folio);

            return v is null ? null : Map(v);
        }

        public async Task<IEnumerable<ValeDto>> GetByInspeccionDetalleAsync(long idDetalle)
        {
            var list = await _context.Vales
                .AsNoTracking()
                .Include(x => x.Pieza)
                .Include(x => x.UsuarioSolicita)
                .Where(x => x.IdInspeccionDetalle == idDetalle)
                .OrderByDescending(x => x.FechaGeneracion)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<ValeDto> GenerarAsync(ValeCreateDto dto, long idUsuario)
        {
            // 1. Validar detalle
            var detalle = await _context.InspeccionDetalles
                .Include(d => d.Inspeccion)
                .Include(d => d.Defecto)
                .FirstOrDefaultAsync(d => d.IdDetalle == dto.IdInspeccionDetalle);

            if (detalle is null)
                throw new InvalidOperationException("El detalle de inspección no existe.");

            // 2. Validar pieza
            var pieza = await _context.Piezas
                .FirstOrDefaultAsync(p => p.IdPieza == dto.IdPieza && p.Activo);

            if (pieza is null)
                throw new InvalidOperationException("La pieza no existe o está inactiva.");

            // 3. Validar permiso de generar vale en la operación de la inspección
            var idOperacion = detalle.Inspeccion.IdOperacion;
            var puedeGenerar = await _context.PermisosOperacion
                .AnyAsync(p => p.IdUsuario == idUsuario
                            && p.IdOperacion == idOperacion
                            && p.PuedeGenerarVale);

            if (!puedeGenerar)
                throw new UnauthorizedAccessException("No tienes permiso para generar vales en esta operación.");

            if (dto.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            // 4. Obtener siguiente folio con el SP
            var folio = await ObtenerSiguienteFolioAsync();

            // 5. Crear vale
            var vale = new Vale
            {
                Folio = folio,
                IdInspeccionDetalle = dto.IdInspeccionDetalle,
                IdPieza = dto.IdPieza,
                Cantidad = dto.Cantidad,
                IdUsuarioSolicita = idUsuario,
                FechaGeneracion = DateTime.UtcNow,
                RutaPdf = null,                    // se puede llenar después al generar PDF
                Estado = "GENERADO"
            };

            _context.Vales.Add(vale);
            await _context.SaveChangesAsync();

            // Recargar navegaciones
            await _context.Entry(vale).Reference(v => v.Pieza).LoadAsync();
            await _context.Entry(vale).Reference(v => v.UsuarioSolicita).LoadAsync();

            return Map(vale);
        }

        public async Task<ValeDto?> ActualizarEstadoAsync(long id, ValeUpdateEstadoDto dto)
        {
            var vale = await _context.Vales
                .Include(x => x.Pieza)
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
            v.IdInspeccionDetalle,
            v.IdPieza,
            v.Pieza.Codigo,
            v.Pieza.Nombre,
            v.Cantidad,
            v.IdUsuarioSolicita,
            v.UsuarioSolicita.Nombre,
            v.FechaGeneracion,
            v.RutaPdf,
            v.Estado
        );
    }
}
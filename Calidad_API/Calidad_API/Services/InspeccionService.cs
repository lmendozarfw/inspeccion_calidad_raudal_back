using Calidad_API.Data;
using Calidad_API.DTOs.Inspecciones;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class InspeccionService : IInspeccionService
    {
        private readonly ApplicationDbContext _context;

        public InspeccionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InspeccionDto?> GetByIdAsync(long id)
        {
            var i = await _context.Inspecciones
                .AsNoTracking()
                .Include(x => x.Transfer)
                .Include(x => x.Operacion)
                .Include(x => x.TipoInspeccion)
                .Include(x => x.Usuario)
                .Include(x => x.Detalles).ThenInclude(d => d.Defecto)
                .FirstOrDefaultAsync(x => x.IdInspeccion == id);

            return i is null ? null : Map(i);
        }

        public async Task<IEnumerable<InspeccionDto>> GetByTransferAsync(long idTransfer)
        {
            var list = await _context.Inspecciones
                .AsNoTracking()
                .Include(x => x.Transfer)
                .Include(x => x.Operacion)
                .Include(x => x.TipoInspeccion)
                .Include(x => x.Usuario)
                .Include(x => x.Detalles).ThenInclude(d => d.Defecto)
                .Where(x => x.IdTransfer == idTransfer)
                .OrderByDescending(x => x.FechaInspeccion)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<IEnumerable<InspeccionDto>> GetByAreaAsync(long idArea, DateTime? desde, DateTime? hasta)
        {
            var query = _context.Inspecciones
                .AsNoTracking()
                .Include(x => x.Transfer)
                .Include(x => x.Operacion)
                .Include(x => x.TipoInspeccion)
                .Include(x => x.Usuario)
                .Include(x => x.Detalles).ThenInclude(d => d.Defecto)
                .Where(x => x.IdOperacion == idArea);

            if (desde.HasValue) query = query.Where(x => x.FechaInspeccion >= desde.Value);
            if (hasta.HasValue) query = query.Where(x => x.FechaInspeccion <= hasta.Value);

            var list = await query
                .OrderByDescending(x => x.FechaInspeccion)
                .Take(100)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<InspeccionDto> CrearAsync(InspeccionCreateDto dto, long idUsuario)
        {
            // Validaciones rápidas
            var transferExiste = await _context.Transfers.AnyAsync(t => t.IdTransfer == dto.IdTransfer);
            if (!transferExiste) throw new InvalidOperationException("El transfer no existe.");

            var operacionExiste = await _context.Operaciones.AnyAsync(o => o.IdOperacion == dto.IdOperacion && o.Activo);
            if (!operacionExiste) throw new InvalidOperationException("La operación no existe o está inactiva.");

            // Verificar permiso del usuario sobre la operación
            var tienePermiso = await _context.PermisosOperacion
                .AnyAsync(p => p.IdUsuario == idUsuario && p.IdOperacion == dto.IdOperacion && p.PuedeCapturar);
            if (!tienePermiso)
                throw new UnauthorizedAccessException("No tienes permiso de captura en esta área.");

            var entity = new Inspeccion
            {
                IdTransfer = dto.IdTransfer,
                IdOperacion = dto.IdOperacion,
                IdTipoInspeccion = dto.IdTipoInspeccion,
                IdUsuario = idUsuario,
                FechaInspeccion = DateTime.UtcNow,
                Dispositivo = dto.Dispositivo,
                Observaciones = dto.Observaciones,
                Estado = "ABIERTA"
            };

            _context.Inspecciones.Add(entity);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(entity.IdInspeccion))!;
        }

        public async Task<InspeccionDetalleDto> AgregarDetalleAsync(long idInspeccion, InspeccionDetalleCreateDto dto, long idUsuario)
        {
            var inspeccion = await _context.Inspecciones
                .FirstOrDefaultAsync(i => i.IdInspeccion == idInspeccion);

            if (inspeccion is null)
                throw new InvalidOperationException("La inspección no existe.");
            if (inspeccion.Estado != "ABIERTA")
                throw new InvalidOperationException("La inspección ya está cerrada.");

            // Validar defecto pertenece a la operación de la inspección
            var defecto = await _context.Defectos
                .FirstOrDefaultAsync(d => d.IdDefecto == dto.IdDefecto && d.DefectosOperacion.Any(dop => dop.IdOperacion == inspeccion.IdOperacion) && d.Activo);
            if (defecto is null)
                throw new InvalidOperationException("El defecto no pertenece a la operación de la inspección o está inactivo.");

            var tipo = dto.TipoRegistro.Trim().ToUpper();
            if (tipo is not ("PIOCHA" or "REPROCESO"))
                throw new InvalidOperationException("TipoRegistro debe ser PIOCHA o REPROCESO.");

            var detalle = new InspeccionDetalle
            {
                IdInspeccion = idInspeccion,
                IdDefecto = dto.IdDefecto,
                TipoRegistro = tipo,
                Lado = string.IsNullOrWhiteSpace(dto.Lado) ? null : dto.Lado.Trim().ToUpper(),
                Cantidad = dto.Cantidad <= 0 ? 1 : dto.Cantidad,
                ValorObjetivo = dto.ValorObjetivo,
                ValorMin = dto.ValorMin,
                ValorMax = dto.ValorMax,
                Unidad = dto.Unidad,
                ValorObtenido = dto.ValorObtenido,
                ResultadoCualitativo = dto.ResultadoCualitativo,
                FechaRegistro = DateTime.UtcNow,
                IdUsuario = idUsuario
            };

            _context.InspeccionDetalles.Add(detalle);
            await _context.SaveChangesAsync();

            // Recargar con defecto
            await _context.Entry(detalle).Reference(d => d.Defecto).LoadAsync();

            return new InspeccionDetalleDto(
                detalle.IdDetalle,
                detalle.IdDefecto,
                detalle.Defecto.Codigo,
                detalle.Defecto.Nombre,
                detalle.TipoRegistro,
                detalle.Lado,
                detalle.Cantidad,
                detalle.ValorObjetivo,
                detalle.ValorMin,
                detalle.ValorMax,
                detalle.Unidad,
                detalle.ValorObtenido,
                detalle.ResultadoCualitativo,
                detalle.FechaRegistro
            );
        }

        public async Task<InspeccionDto?> CerrarAsync(long idInspeccion, InspeccionCerrarDto dto)
        {
            var inspeccion = await _context.Inspecciones.FindAsync(idInspeccion);
            if (inspeccion is null) return null;
            if (inspeccion.Estado == "CERRADA")
                throw new InvalidOperationException("La inspección ya está cerrada.");

            inspeccion.Estado = "CERRADA";
            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                inspeccion.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(idInspeccion);
        }

        public async Task<bool> EliminarDetalleAsync(long idDetalle)
        {
            var detalle = await _context.InspeccionDetalles
                .Include(d => d.Inspeccion)
                .FirstOrDefaultAsync(d => d.IdDetalle == idDetalle);

            if (detalle is null) return false;
            if (detalle.Inspeccion.Estado != "ABIERTA")
                throw new InvalidOperationException("No se puede eliminar detalle de una inspección cerrada.");

            _context.InspeccionDetalles.Remove(detalle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InspectionRegisterResponse> RegistrarAsync(
    InspectionRegisterRequest request,
    long idUsuario)
        {
            if (request.Contexto is null)
                throw new InvalidOperationException("El contexto es obligatorio.");

            var programa = request.Contexto.Programa?.Trim();
            var lote = request.Contexto.Lote?.Trim();

            if (string.IsNullOrWhiteSpace(programa))
                throw new InvalidOperationException("El programa es obligatorio.");
            if (string.IsNullOrWhiteSpace(lote))
                throw new InvalidOperationException("El lote es obligatorio.");
            if (request.Inspecciones is null || request.Inspecciones.Count == 0)
                throw new InvalidOperationException("Debes enviar al menos una inspección.");

            // —— Modelo opcional (código combinación o base) ——
            int? idModelo = null;
            string? modeloCodigo = null;
            if (!string.IsNullOrWhiteSpace(request.Contexto.Modelo))
            {
                var codigo = request.Contexto.Modelo.Trim();
                var modelo = await _context.Modelos.AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.Estatus &&
                        (m.CodigoCombinacion == codigo || m.CodigoMB == codigo));

                if (modelo is not null)
                {
                    idModelo = modelo.IdModelo;
                    modeloCodigo = modelo.CodigoCombinacion;
                }
                // Si no existe, se ignora (no obliga registro de modelo)
            }

            // —— Transfer: buscar por lote; crear o actualizar ——
            var transfer = await _context.Transfers
                .Include(t => t.Modelo)
                .FirstOrDefaultAsync(t => t.Lote == lote);

            if (transfer is null)
            {
                transfer = new Transfer
                {
                    QrRaw = $"LOTE:{lote}|PROG:{programa}",
                    Programa = programa,
                    Lista = string.IsNullOrWhiteSpace(request.Contexto.Lista)
                        ? null
                        : request.Contexto.Lista.Trim(),
                    Lote = lote,
                    Punto = string.IsNullOrWhiteSpace(request.Contexto.Punto)
                        ? null
                        : request.Contexto.Punto.Trim(),
                    IdModelo = idModelo,
                    FechaPrimerScan = DateTime.UtcNow,
                    FechaUltimoScan = DateTime.UtcNow
                };
                _context.Transfers.Add(transfer);
            }
            else
            {
                transfer.FechaUltimoScan = DateTime.UtcNow;
                transfer.Programa = programa;
                if (!string.IsNullOrWhiteSpace(request.Contexto.Lista))
                    transfer.Lista = request.Contexto.Lista.Trim();
                if (!string.IsNullOrWhiteSpace(request.Contexto.Punto))
                    transfer.Punto = request.Contexto.Punto.Trim();
                if (idModelo.HasValue)
                    transfer.IdModelo = idModelo;
            }

            await _context.SaveChangesAsync();

            var inspeccionesCreadas = new List<InspectionCreatedDto>();

            foreach (var item in request.Inspecciones)
            {
                var operacionOk = await _context.Operaciones
                    .AnyAsync(o => o.IdOperacion == item.IdOperacion && o.Activo);
                if (!operacionOk)
                    throw new InvalidOperationException($"La operación {item.IdOperacion} no existe o está inactiva.");

                var tipoOk = await _context.TiposInspeccion
                    .AnyAsync(t => t.IdTipoInspeccion == item.IdTipoInspeccion);
                if (!tipoOk)
                    throw new InvalidOperationException($"El tipo de inspección {item.IdTipoInspeccion} no existe.");

                var puedeCapturar = await _context.PermisosOperacion.AnyAsync(p =>
                    p.IdUsuario == idUsuario
                    && p.IdOperacion == item.IdOperacion
                    && p.PuedeCapturar);
                if (!puedeCapturar)
                    throw new UnauthorizedAccessException(
                        $"No tienes permiso de captura en la operación {item.IdOperacion}.");

                var inspeccion = new Inspeccion
                {
                    IdTransfer = transfer.IdTransfer,
                    IdOperacion = item.IdOperacion,
                    IdTipoInspeccion = item.IdTipoInspeccion,
                    IdUsuario = idUsuario,
                    FechaInspeccion = DateTime.UtcNow,
                    Dispositivo = item.Dispositivo,
                    Observaciones = item.Observaciones,
                    Estado = "ABIERTA"
                };

                _context.Inspecciones.Add(inspeccion);
                await _context.SaveChangesAsync();

                var detallesCreados = new List<InspectionDetalleCreatedDto>();

                if (item.Defectos is { Count: > 0 })
                {
                    foreach (var def in item.Defectos)
                    {
                        var defecto = await _context.Defectos
                            .FirstOrDefaultAsync(d =>
                                d.IdDefecto == def.IdDefecto
                                && d.Activo
                                && d.DefectosOperacion.Any(dop => dop.IdOperacion == item.IdOperacion));

                        if (defecto is null)
                            throw new InvalidOperationException(
                                $"El defecto {def.IdDefecto} no pertenece a la operación {item.IdOperacion} o está inactivo.");

                        var lado = MapLado(def.Lado);
                        var tipoReg = MapTipoRegistro(def.TipoRegistro);

                        var detalle = new InspeccionDetalle
                        {
                            IdInspeccion = inspeccion.IdInspeccion,
                            IdDefecto = def.IdDefecto,
                            TipoRegistro = tipoReg,
                            Lado = lado,
                            Cantidad = 1,
                            FechaRegistro = DateTime.UtcNow,
                            IdUsuario = idUsuario
                        };

                        _context.InspeccionDetalles.Add(detalle);
                        await _context.SaveChangesAsync();

                        detallesCreados.Add(new InspectionDetalleCreatedDto(
                            detalle.IdDetalle,
                            detalle.IdDefecto,
                            detalle.TipoRegistro,
                            detalle.Lado,
                            detalle.Cantidad));
                    }
                }

                inspeccionesCreadas.Add(new InspectionCreatedDto(
                    inspeccion.IdInspeccion,
                    inspeccion.IdOperacion,
                    inspeccion.IdTipoInspeccion,
                    inspeccion.Estado,
                    detallesCreados));
            }

            return new InspectionRegisterResponse(
                transfer.IdTransfer,
                transfer.Lote,
                transfer.Programa ?? programa,
                transfer.IdModelo,
                modeloCodigo ?? transfer.Modelo?.CodigoCombinacion,
                inspeccionesCreadas);
        }

        private static string? MapLado(string? lado)
        {
            if (string.IsNullOrWhiteSpace(lado)) return null;

            return lado.Trim().ToLowerInvariant() switch
            {
                "derecho" or "derecho" => "DERECHO",
                "izquierdo" => "IZQUIERDO",
                "ambos" or "par" => "PAR",                
                _ => lado.Trim().ToUpperInvariant() switch
                {
                    "DERECHO" or "IZQUIERDO" or "PAR" => lado.Trim().ToUpperInvariant(),
                    _ => throw new InvalidOperationException(
                        "Lado inválido. Use: derecho, izquierdo o ambos.")
                }
            };
        }

        private static string MapTipoRegistro(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return "PIOCHA";

            return tipo.Trim().ToLowerInvariant() switch
            {
                "piocha" => "PIOCHA",
                "reproceso" => "REPROCESO",
                _ => tipo.Trim().ToUpperInvariant() switch
                {
                    "PIOCHA" or "REPROCESO" => tipo.Trim().ToUpperInvariant(),
                    _ => throw new InvalidOperationException(
                        "TipoRegistro inválido. Use: piocha o reproceso.")
                }
            };
        }

        private static InspeccionDto Map(Inspeccion i) => new(
            i.IdInspeccion,
            i.IdTransfer,
            i.Transfer.Lote,
            i.IdOperacion,
            i.Operacion.Codigo,
            i.Operacion.Nombre,
            i.IdTipoInspeccion,
            i.TipoInspeccion.Codigo,
            i.IdUsuario,
            i.Usuario.Nombre,
            i.FechaInspeccion,
            i.Dispositivo,
            i.Observaciones,
            i.Estado,
            i.Detalles.Select(d => new InspeccionDetalleDto(
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
        );
    }
}
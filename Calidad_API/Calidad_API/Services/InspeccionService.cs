using Calidad_API.Data;
using Calidad_API.DTOs.Inspecciones;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using DocumentFormat.OpenXml.Math;
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

        public async Task<IEnumerable<InspeccionDto>> GetAll()
        {
            var list = await _context.Inspecciones
                    .AsNoTracking()
                    .Include(x => x.Transfer)
                    .Include(x => x.Operacion)
                    .Include(x => x.TipoInspeccion)
                    .Include(x => x.Usuario)
                    .Include(x => x.Detalles).ThenInclude(d => d.Defecto)
                    .OrderByDescending(x => x.FechaInspeccion)
                    .ToListAsync();
            return list.Select(Map);
        }

        public async Task<IEnumerable<InspeccionDto>> Search(string? search)
        {
            search = Limpiar(search);

            var query = _context.Inspecciones.AsNoTracking()
                    .Include(x => x.Transfer)
                    .Include(x => x.Operacion)
                    .Include(x => x.TipoInspeccion)
                    .Include(x => x.Usuario)
                    .Include(x => x.Detalles).ThenInclude(d => d.Defecto)
                    .AsQueryable();

            if (search is not null)
            {
                query = query.Where(i =>
                        (i.Transfer.Programa != null && i.Transfer.Programa.Contains(search))
                     || i.Transfer.Lote.Contains(search)
                     || i.Operacion.Nombre.Contains(search)
                     || i.Operacion.Codigo.Contains(search)
                     || i.TipoInspeccion.Codigo.Contains(search)
                     || i.Usuario.Nombre.Contains(search)
                     || i.Detalles.Any(d =>
                            d.Defecto.Nombre.Contains(search)
                         || d.Defecto.Codigo.Contains(search)));
            }

            var list = await query
                .OrderByDescending(x => x.FechaInspeccion)
                .ToListAsync();

            return list.Select(Map);
        }

        public async Task<List<InspeccionDto>> GetAbiertasByTransferAsync(string programa, string lote, string modelo)
        {
            programa = programa.Trim();
            lote = lote.Trim();
            var modeloFound = await _context.Modelos.Where(m => m.CodigoMB.Contains(modelo) || m.CodigoCombinacion.Contains(modelo)).FirstOrDefaultAsync();
            Console.WriteLine("========================VEAMOS=====================");
            Console.WriteLine(modeloFound != null ? modeloFound.IdModelo : "null");
            Console.WriteLine("========================FIN=====================");
            return await _context.Inspecciones
                .AsNoTracking()
                .Where(x =>
                    x.Estado == "ABIERTA"
                    && x.Transfer.Programa == programa
                    && x.Transfer.Lote == lote
                    && x.Transfer.IdModelo == (modeloFound != null ? modeloFound.IdModelo : null)
                    && x.Detalles.Any(d => d.TipoRegistro == "PIOCHA"))
                .OrderByDescending(x => x.FechaInspeccion)
                .Select(x => new InspeccionDto(
                    x.IdInspeccion,
                    x.IdTransfer,
                    x.Transfer.Lote,
                    x.IdOperacion,
                    x.Operacion.Codigo,
                    x.Operacion.Nombre,
                    x.IdTipoInspeccion,
                    x.TipoInspeccion.Codigo,
                    x.IdUsuario,
                    x.Usuario.Nombre,
                    x.FechaInspeccion,
                    x.Dispositivo,
                    x.Observaciones,
                    x.Estado,

                    x.Detalles
                        .Where(d => d.TipoRegistro == "PIOCHA")
                        .Select(d => new InspeccionDetalleDto(
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
                        .ToList()
                ))
                .ToListAsync();
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

            var insp = await _context.Inspecciones
                        .Include(i => i.Transfer)
                        .FirstAsync(i => i.IdInspeccion == idInspeccion);

            await EncolarSolicitudSiAplicaAsync(insp, detalle, defecto, idUsuario);

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

                        await EncolarSolicitudSiAplicaAsync(inspeccion, detalle, defecto, idUsuario);

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

        private static string? Limpiar(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

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

        private static InspeccionDto MapWithOnlyPiochaDetails(Inspeccion i) => new(
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
            i.Detalles.Where(d => d.TipoRegistro == "PIOCHA").Select(d => new InspeccionDetalleDto(
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

        public async Task<IEnumerable<InspeccionReporteDto>> GetReport(
            DateTime desde, DateTime hasta
        )
        {
            return await _context.Inspecciones
            .Include(x => x.Transfer)
            .Include(x => x.Usuario)
            .Include(x => x.TipoInspeccion)
            .Include(x => x.Detalles).AsNoTracking()
            .Where(i => i.FechaInspeccion >= desde && i.FechaInspeccion < hasta.AddDays(1))
            .Select(i => new InspeccionReporteDto(
                i.IdInspeccion,
                i.FechaInspeccion,
                i.Transfer.Programa ?? "",
                i.Transfer.Lote,
                i.Operacion.Nombre,
                i.TipoInspeccion.Nombre,
                i.Usuario.Nombre,
                i.Estado,
                i.Dispositivo ?? "",
                i.Detalles.Count(),
                i.Observaciones ?? ""
            )).ToListAsync();
        }

        public async Task<IEnumerable<InspeccionPorLoteReporteDto>> GetReportByLotes(
            DateTime desde, DateTime hasta
        )
        {
            var list = await _context.Inspecciones
                .AsNoTracking()
                .AsSplitQuery()
                .Include(i => i.Transfer)
                .Include(i => i.Detalles)
                .Include(i => i.Vales)
                .Where(i => i.FechaInspeccion >= desde && i.FechaInspeccion < hasta.AddDays(1))
                .ToListAsync();

            return list.GroupBy(i => i.Transfer.Lote)
                        .Select(g => new InspeccionPorLoteReporteDto(
                            g.First().Transfer.Programa ?? "",
                            g.Key,
                            g.Count(),
                            (int)g.SelectMany(x => x.Detalles).Sum(d => d.Cantidad),
                            (int)g.SelectMany(x => x.Detalles).Where(d => d.TipoRegistro == "PIOCHA").Sum(d => d.Cantidad),
                            (int)g.SelectMany(x => x.Detalles).Where(d => d.TipoRegistro == "REPROCESO").Sum(d => d.Cantidad),
                            g.SelectMany(x => x.Vales).Count()
                        ))
                        .OrderBy(x => x.Lote)
                        .ToList();
        }

        public async Task<InpseccionIndicadoresReporteDto> GetReportIndicadores(DateTime desde, DateTime hasta)
        {
            var inspecciones = await _context.Inspecciones.AsNoTracking()
            .Include(x => x.Detalles)
            .Include(x => x.Vales)
            .Where(x => x.FechaInspeccion >= desde && x.FechaInspeccion < hasta.AddDays(1)).ToListAsync();
            int inspeccionesCount = inspecciones.Count();
            int defectos = (int)inspecciones.SelectMany(i => i.Detalles).Sum(d => d.Cantidad);
            int piochas = (int)inspecciones.SelectMany(i => i.Detalles).Where(x => x.TipoRegistro == "PIOCHA").Sum(d => d.Cantidad);
            int reprocesos = (int)inspecciones.SelectMany(i => i.Detalles).Where(x => x.TipoRegistro == "REPROCESO").Sum(d => d.Cantidad);
            int vales = inspecciones.SelectMany(i => i.Vales).Count();
            return new InpseccionIndicadoresReporteDto(
                inspeccionesCount,
                defectos,
                piochas,
                reprocesos,
                vales,
                inspeccionesCount > 0 ? Math.Round(defectos / (decimal)inspeccionesCount, 2) : 0
            );
        }

        public async Task<IEnumerable<InspeccionPorOperacionReporteDto>> GetReportePorOpracion(DateTime desde, DateTime hasta)
        {
            var inspecciones = await _context.Inspecciones
        .AsNoTracking()
        .Include(x => x.Operacion)
        .Include(x => x.Transfer)
        .Include(x => x.TipoInspeccion)
        .Include(x => x.Detalles).ThenInclude(d => d.Defecto).ThenInclude(d => d.Criticidad)
        .Where(x => x.FechaInspeccion >= desde && x.FechaInspeccion < hasta.AddDays(1))
        .ToListAsync();

            return inspecciones
                .GroupBy(x => x.IdOperacion)
                .Select(g => new InspeccionPorOperacionReporteDto(
                    g.First().Operacion.Codigo,
                    g.First().Operacion.Nombre,
                    g.Count(),
                    g.SelectMany(x => x.Detalles)
                     .GroupBy(d => d.Defecto.Codigo)
                     .Select(dg => new InspeccionPorOperacionDefectoReportDto(
                         dg.Key,
                         dg.First().Defecto.Nombre,
                         dg.First().Defecto.Criticidad?.Nombre ?? "",
                         (int)dg.Where(d => d.TipoRegistro == "PIOCHA").Sum(d => d.Cantidad),
                         (int)dg.Where(d => d.TipoRegistro == "REPROCESO").Sum(d => d.Cantidad)
                     ))
                     .ToList()
                ))
                .OrderBy(x => x.CodigoOperacion)
                .ToList();
        }

        private async Task EncolarSolicitudSiAplicaAsync(
    Inspeccion inspeccion,
    InspeccionDetalle detalle,
    Defecto defecto,
    long idUsuario)
        {
            if (!defecto.AplicaPieza || !defecto.IdPieza.HasValue || defecto.IdPieza.Value <= 0)
                return;

            // Cargar transfer si no viene
            if (inspeccion.Transfer is null)
                await _context.Entry(inspeccion).Reference(i => i.Transfer).LoadAsync();

            var transfer = inspeccion.Transfer
                ?? throw new InvalidOperationException("La inspección no tiene transfer.");

            var solicitud = new SolicitudMaterial
            {
                IdInspeccion = inspeccion.IdInspeccion,
                IdInspeccionDetalle = detalle.IdDetalle,
                IdOperacion = inspeccion.IdOperacion,
                Programa = transfer.Programa,
                Lote = transfer.Lote,
                Lista = transfer.Lista,
                Punto = transfer.Punto,
                Lado = detalle.Lado,
                IdPieza = defecto.IdPieza.Value,
                Cantidad = detalle.Cantidad <= 0 ? 1 : detalle.Cantidad,
                IdUsuarioSolicita = idUsuario,
                FechaSolicitud = DateTime.UtcNow,
                Estado = "PENDIENTE"
            };

            _context.SolicitudesMaterial.Add(solicitud);
            await _context.SaveChangesAsync();
        }
    }
}
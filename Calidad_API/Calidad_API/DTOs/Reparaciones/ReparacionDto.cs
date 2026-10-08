using Calidad_API.DTOs.Inspecciones;

public record ReparacionDto(
    long IdReparacion,
    long IdUsuario,
    string NombreUsuario,
    string Username,
    long IdInspeccion,
    DateTime FechaInicio,
    DateTime FechaFin,
    List<InspeccionDetalleDto> Detalles
);
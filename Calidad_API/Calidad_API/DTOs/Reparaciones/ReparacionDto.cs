public record ReparacionDto(
    long IdReparacion,
    long IdUsuario,
    string NombreUsuario,
    string Username,
    long IdInspeccionDetalle,
    string NombreDefecto,
    DateTime FechaInicio,
    DateTime FechaFin
);
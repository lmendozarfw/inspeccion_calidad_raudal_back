namespace Calidad_API.DTOs.CodigoAutorizacion;

public record CodigoAutorizacionDto(
    string? Codigo,
    DateTime FechaCreacion,
    DateTime FechaExpiracion,
    bool Activo
    );
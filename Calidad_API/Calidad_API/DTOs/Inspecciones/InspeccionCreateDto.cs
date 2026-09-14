namespace Calidad_API.DTOs.Inspecciones
{
    public record InspeccionCreateDto(
        long IdTransfer,
        long IdArea,
        short IdTipoInspeccion,
        string? Dispositivo,
        string? Observaciones
    );
}

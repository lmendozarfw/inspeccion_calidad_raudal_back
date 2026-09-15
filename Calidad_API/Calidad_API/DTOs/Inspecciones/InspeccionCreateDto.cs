namespace Calidad_API.DTOs.Inspecciones
{
    public record InspeccionCreateDto(
        long IdTransfer,
        long IdOperacion,
        short IdTipoInspeccion,
        string? Dispositivo,
        string? Observaciones
    );
}

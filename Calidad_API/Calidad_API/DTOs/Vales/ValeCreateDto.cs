namespace Calidad_API.DTOs.Vales
{
    public record ValeCreateDto(
        long IdInspeccion,
        IReadOnlyList<ValeLineaCreateDto>? Lineas = null
    );
}

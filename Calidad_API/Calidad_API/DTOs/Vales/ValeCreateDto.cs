namespace Calidad_API.DTOs.Vales
{
    public record ValeCreateDto(
        long IdInspeccion,
        string CodigoAutorizacion,
        IReadOnlyList<ValeLineaCreateDto>? Lineas = null
    );
}

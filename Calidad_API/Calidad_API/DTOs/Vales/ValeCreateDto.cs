namespace Calidad_API.DTOs.Vales
{
    public record ValeCreateDto(
        long IdInspeccionDetalle,
        long IdPieza,
        decimal Cantidad
    );
}

namespace Calidad_API.DTOs.Vales
{
    public record ValeLineaDto(
        long IdValeDetalle,
        long IdPieza,
        string PiezaCodigo,
        string PiezaNombre,
        decimal Cantidad
    );
}

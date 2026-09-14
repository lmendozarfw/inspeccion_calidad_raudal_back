namespace Calidad_API.DTOs.Transfers
{
    public record TransferCreateDto(
        string QrRaw,
        string? Programa,
        string? Lista,
        string Lote,
        string? Punto,
        long? IdModelo
    );
}

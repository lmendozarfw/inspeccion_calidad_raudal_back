namespace Calidad_API.DTOs.Transfers
{
    public record TransferDto(
        long IdTransfer,
        string QrRaw,
        string? Programa,
        string? Lista,
        string Lote,
        string? Punto,
        long? IdModelo,
        string? ModeloCodigo,
        string? ModeloNombre,
        DateTime FechaPrimerScan,
        DateTime FechaUltimoScan
    );
}

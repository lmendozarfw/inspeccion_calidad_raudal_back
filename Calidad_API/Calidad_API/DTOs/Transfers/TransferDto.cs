namespace Calidad_API.DTOs.Transfers
{
    public record TransferDto(
        long IdTransfer,
        string QrRaw,
        string? Programa,
        string? Lista,
        string Lote,
        string? Punto,
        int? IdModelo,
        string? CodigoModeloBase,
        string? CodigoCombinacion,
        string? Descripcion,
        DateTime FechaPrimerScan,
        DateTime FechaUltimoScan
    );
}

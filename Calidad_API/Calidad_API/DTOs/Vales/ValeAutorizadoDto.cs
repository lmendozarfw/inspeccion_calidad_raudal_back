namespace Calidad_API.DTOs.Vales
{
    public record ValeAutorizadoDto(
        long IdVale,
        string Folio,
        string Estado,
        long IdInspeccion,
        string? NombreOperacion,
        string? Programa,
        string? Lote,
        string SolicitaNombre,
        string AutorizaNombre,
        DateTime FechaGeneracion,
        DateTime? FechaAutorizacion,
        DateTime FechaInspeccion,
        string? PdfUrl
    );
}

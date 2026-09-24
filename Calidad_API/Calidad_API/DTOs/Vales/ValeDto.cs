namespace Calidad_API.DTOs.Vales
{
    public record ValeDto(
        long IdVale,
        string Folio,
        long IdInspeccion,
        long IdUsuarioSolicita,
        string UsuarioNombre,
        DateTime FechaGeneracion,
        string? RutaPdf,
        string Estado,
        IReadOnlyList<ValeLineaDto> Lineas
    );
}

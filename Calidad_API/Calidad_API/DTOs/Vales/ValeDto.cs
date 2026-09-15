namespace Calidad_API.DTOs.Vales
{
    public record ValeDto(
        long IdVale,
        string Folio,
        long IdInspeccionDetalle,
        long IdPieza,
        string PiezaCodigo,
        string PiezaNombre,
        decimal Cantidad,
        long IdUsuarioSolicita,
        string UsuarioNombre,
        DateTime FechaGeneracion,
        string? RutaPdf,
        string Estado
    );
}

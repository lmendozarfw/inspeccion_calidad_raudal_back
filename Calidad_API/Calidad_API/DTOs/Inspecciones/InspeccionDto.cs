namespace Calidad_API.DTOs.Inspecciones
{
    public record InspeccionDto(
        long IdInspeccion,
        long IdTransfer,
        string Lote,
        long IdOperacion,
        string OperacionCodigo,
        string OperacionNombre,
        short IdTipoInspeccion,
        string TipoInspeccionCodigo,
        long IdUsuario,
        string UsuarioNombre,
        DateTime FechaInspeccion,
        string? Dispositivo,
        string? Observaciones,
        string Estado,
        IEnumerable<InspeccionDetalleDto> Detalles
    );
}

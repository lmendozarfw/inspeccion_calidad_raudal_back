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

    public record InspeccionReporteDto(
        long IdInspeccion,
        DateTime FechaInspeccion,
        string Programa,
        string Lote,
        string Operacion,
        string Tipo,
        string Usuario,
        string Estado,
        string Dispositivo,
        int NumeroDefectos,
        string Observaciones
    );

    public record InspeccionPorLoteReporteDto(
        string Programa,
        string Lote,
        int Inspecciones,
        int Defectos,
        int Piochas,
        int Reprocesos,
        int Vales
    );

    public record InpseccionIndicadoresReporteDto(
        int Inspecciones,
        int Defectos,
        int Piochas,
        int Reprocesos,
        int ValesGenerados,
        decimal DefectosPorInspeccion
    );
}
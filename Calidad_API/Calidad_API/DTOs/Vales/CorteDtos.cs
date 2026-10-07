namespace Calidad_API.DTOs.Vales
{
    public record CorteEjecutarResponse(
        int CortesCreados,
        IReadOnlyList<CorteResumenDto> Cortes
    );

    public record CorteResumenDto(
        long IdCorte,
        long IdOperacion,
        string OperacionCodigo,
        string OperacionNombre,
        DateTime FechaCorte,
        string Estado,
        int NumSolicitudes,
        decimal CantidadTotal
    );

    public record CorteDetalleDto(
        long IdCorte,
        long IdOperacion,
        string OperacionCodigo,
        string OperacionNombre,
        DateTime FechaCorte,
        string Estado,
        long? IdUsuarioAutoriza,
        DateTime? FechaAutorizacion,
        long? IdVale,
        IReadOnlyList<CorteLineaDto> Lineas
    );

    public record CorteLineaDto(
        long IdSolicitud,
        string? Programa,
        string Lote,
        string? Lado,
        long IdPieza,
        string PiezaCodigo,
        string PiezaNombre,
        decimal Cantidad,
        DateTime FechaSolicitud
    );

    public record CorteAutorizarDto(string CodigoAutorizacion);
}
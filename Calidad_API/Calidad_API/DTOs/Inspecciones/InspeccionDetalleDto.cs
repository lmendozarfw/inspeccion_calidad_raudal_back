namespace Calidad_API.DTOs.Inspecciones
{
    public record InspeccionDetalleDto(
        long IdDetalle,
        long IdDefecto,
        string DefectoCodigo,
        string DefectoNombre,
        string TipoRegistro,          // PIOCHA | REPROCESO
        string? Lado,                 // IZQUIERDO | DERECHO | PAR
        decimal Cantidad,
        decimal? ValorObjetivo,
        decimal? ValorMin,
        decimal? ValorMax,
        string? Unidad,
        decimal? ValorObtenido,
        string? ResultadoCualitativo,
        DateTime FechaRegistro
    );
}

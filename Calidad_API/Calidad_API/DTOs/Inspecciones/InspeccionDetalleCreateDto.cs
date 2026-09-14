namespace Calidad_API.DTOs.Inspecciones
{
    public record InspeccionDetalleCreateDto(
         long IdDefecto,
         string TipoRegistro,          // PIOCHA | REPROCESO
         string? Lado,
         decimal Cantidad,
         decimal? ValorObjetivo,
         decimal? ValorMin,
         decimal? ValorMax,
         string? Unidad,
         decimal? ValorObtenido,
         string? ResultadoCualitativo
     );
}

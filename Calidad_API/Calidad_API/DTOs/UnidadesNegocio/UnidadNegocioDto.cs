namespace Calidad_API.DTOs.UnidadesNegocio
{
    public record UnidadNegocioDto(long IdUnidadNegocio, string Codigo, string Nombre, bool Activo, DateTime FechaAlta);
    public record UnidadNegocioCreateDto(string Codigo, string Nombre);
    public record UnidadNegocioUpdateDto(string Nombre, bool Activo);
}
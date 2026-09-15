namespace Calidad_API.DTOs.TiposInspeccion
{
    public record TipoInspeccionDto(short IdTipoInspeccion, string Codigo, string Nombre, bool Activo);
    public record TipoInspeccionCreateDto(short IdTipoInspeccion, string Codigo, string Nombre);
    public record TipoInspeccionUpdateDto(string Nombre, bool Activo);
}
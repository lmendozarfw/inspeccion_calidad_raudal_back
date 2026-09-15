namespace Calidad_API.DTOs.Modelos
{
    public record ModeloDto(long IdModelo, string Codigo, string Nombre, string? Familia, bool Activo, DateTime FechaAlta);
    public record ModeloCreateDto(string Codigo, string Nombre, string? Familia);
    public record ModeloUpdateDto(string Nombre, string? Familia, bool Activo);
}
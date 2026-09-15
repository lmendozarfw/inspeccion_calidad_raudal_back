namespace Calidad_API.DTOs.Piezas
{
    public record PiezaDto(long IdPieza, string Codigo, string Nombre, string? Unidad, bool Activo, DateTime FechaAlta);
    public record PiezaCreateDto(string Codigo, string Nombre, string? Unidad);
    public record PiezaUpdateDto(string Nombre, string? Unidad, bool Activo);
}
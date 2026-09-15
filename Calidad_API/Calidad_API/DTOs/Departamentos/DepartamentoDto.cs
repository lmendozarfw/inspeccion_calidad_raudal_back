namespace Calidad_API.DTOs.Departamentos
{
    public record DepartamentoDto(long IdDepartamento, string Codigo, string Nombre, long? IdUnidadNegocio, string? UnidadNegocioNombre, bool Activo, DateTime FechaAlta);
    public record DepartamentoCreateDto(string Codigo, string Nombre, long? IdUnidadNegocio);
    public record DepartamentoUpdateDto(string Nombre, long? IdUnidadNegocio, bool Activo);
}
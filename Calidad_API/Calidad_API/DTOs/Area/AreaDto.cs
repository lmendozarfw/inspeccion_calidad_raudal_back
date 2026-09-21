namespace Calidad_API.DTOs.Area
{
    public record AreaDto(
        long IdArea,
        string Codigo,
        string Nombre,
        string Proceso,
        bool Activo,
        long? IdCentroTrabajo,
        long? IdDepartamento,
        string? Departamento
    );
}

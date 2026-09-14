namespace Calidad_API.DTOs.Area
{
    public record AreaUpdateDto(
        string Nombre,
        string Proceso,
        bool Activo,
        long? IdCentroTrabajo,
        long? IdDepartamento
    );
}

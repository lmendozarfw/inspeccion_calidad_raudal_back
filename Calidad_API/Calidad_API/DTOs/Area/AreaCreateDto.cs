namespace Calidad_API.DTOs.Area
{
    public record AreaCreateDto(
        string Codigo,
        string Nombre,
        string Proceso,
        long? IdCentroTrabajo,
        long? IdDepartamento
    );
}

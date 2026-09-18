namespace Calidad_API.DTOs.Operaciones
{
    public record OperacionUpdateDto(
           string Nombre,
           string Proceso,
           bool Activo,
           long? IdDepartamento,
           long? IdUnidadNegocio
       );
}

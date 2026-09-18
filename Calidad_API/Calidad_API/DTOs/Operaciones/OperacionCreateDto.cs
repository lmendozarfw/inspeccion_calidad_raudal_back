namespace Calidad_API.DTOs.Operaciones
{
    public record OperacionCreateDto(
        string Codigo,
        string Nombre,
        string Proceso,
        long? IdDepartamento,
        long? IdUnidadNegocio
    );
}

namespace Calidad_API.DTOs.Operaciones
{
    public record OperacionDto(
        long IdOperacion,
        string Codigo,
        string Nombre,
        string Proceso,
        bool Activo,
        long? IdDepartamento,
        long? IdUnidadNegocio
    );
}

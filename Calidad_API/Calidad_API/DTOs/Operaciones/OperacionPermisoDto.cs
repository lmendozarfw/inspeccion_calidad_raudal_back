namespace Calidad_API.DTOs.Operaciones
{
    public record OperacionPermisoDto(
        long IdOperacion,
        string Codigo,
        string Nombre,
        string Proceso,
        bool PuedeCapturar,
        bool PuedeConsultar,
        bool PuedeGenerarVale
    );
}

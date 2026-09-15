namespace Calidad_API.DTOs
{
    public record LoginRequestDto(string Usuario, string Password);

    public record LoginResponseDto(
        string Token,
        string Usuario,
        string Nombre,
        IEnumerable<string> Roles,
        IEnumerable<PermisoAreaDto> Permisos
    );

    public record PermisoAreaDto(long IdArea, string nombre, string codigo, bool PuedeCapturar, bool PuedeConsultar, bool PuedeGenerarVale);
}

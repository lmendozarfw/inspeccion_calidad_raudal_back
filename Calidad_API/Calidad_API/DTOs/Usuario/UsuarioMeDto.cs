namespace Calidad_API.DTOs.Usuario;

public record UsuarioMeDto(
    long IdUsuario,
    string Usuario,
    string NombreCompleto,
    IEnumerable<string> Roles,
    IEnumerable<PermisoAreaDto> Permisos
);
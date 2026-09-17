namespace Calidad_API.DTOs.Usuario;

public class CreateUsuarioDto
{
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<short> IdsRol { get; set; } = [];
}
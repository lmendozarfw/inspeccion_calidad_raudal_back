namespace Calidad_API.DTOs.Usuario;

public class UpdateUsuarioDto
{
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public List<short> IdsRol { get; set; } = [];
}
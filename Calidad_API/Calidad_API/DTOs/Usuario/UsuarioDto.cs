using Calidad_API.Models;

namespace Calidad_API.DTOs.Usuario;

public class UsuarioDto
{
    public long IdUsuario { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public List<short> IdsRol { get; set; } = [];
    public List<Rol> Roles { get; set; } = [];
    public List<long> IdsOperacion { get; set; } = [];
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
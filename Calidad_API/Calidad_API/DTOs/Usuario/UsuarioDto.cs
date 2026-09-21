using Calidad_API.DTOs.Operaciones;
using Calidad_API.DTOs.Roles;

namespace Calidad_API.DTOs.Usuario;

public class UsuarioDto
{
    public long IdUsuario { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public List<short> IdsRol { get; set; } = [];
    public List<RolDto> Roles { get; set; } = [];
    public List<long> IdsOperacion { get; set; } = [];
    public List<OperacionDto> Operaciones { get; set; } = [];
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
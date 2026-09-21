using Calidad_API.DTOs.Usuario;
using Calidad_API.Models;

namespace Calidad_API.Interfaces
{
    public interface IUserRepository
    {
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<Usuario?> GetByIdWithDetailsAsync(long id);
        Task<List<UsuarioDto>> GetAllAsync();
        Task<UsuarioDto?> GetByIdAsync(long id);
        Task<UsuarioDto?> CreateAsync(CreateUsuarioDto user);
        Task<UsuarioDto?> UpdateAsync(long id, UpdateUsuarioDto user);
        Task<UsuarioDto?> ToggleAsync(long id, ToggleActivoDto toggle);
        Task<UsuarioDto?> AsignarOperacionesAsync(long idUsuario, AsignarOperacionesDto dto);
        Task<bool> CambiarPasswordAsync(long id, CambiarPasswordDto dto);
    }
}

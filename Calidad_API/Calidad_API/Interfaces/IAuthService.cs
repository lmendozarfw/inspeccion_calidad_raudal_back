using Calidad_API.DTOs;
using Calidad_API.DTOs.Usuario;

namespace Calidad_API.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<UsuarioMeDto?> ObtenerUsuarioActualAsync(long idUsuario);
    }
}
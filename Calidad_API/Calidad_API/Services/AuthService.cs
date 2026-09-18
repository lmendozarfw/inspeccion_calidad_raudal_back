using Calidad_API.DTOs;
using Calidad_API.DTOs.Usuario;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Calidad_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<Models.Usuario> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<Models.Usuario> passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Usuario);
            if (user == null)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var token = _tokenService.GenerateToken(user);

            var roles = user.UsuarioRoles.Select(ur => ur.Rol.Codigo).ToList();
            var permisos = user.PermisosOperacion.Select(p => new PermisoAreaDto(
                p.IdOperacion,
                p.Operacion.Nombre,
                p.Operacion.Codigo,
                p.PuedeCapturar,
                p.PuedeConsultar,
                p.PuedeGenerarVale
            )).ToList();

            return new LoginResponseDto(token, user.IdUsuario,user.Username, user.Nombre, roles, permisos);
        }

        public async Task<UsuarioMeDto?> ObtenerUsuarioActualAsync(long idUsuario)
        {
            var user = await _userRepository.GetByIdWithDetailsAsync(idUsuario);
            if (user is null)
            {
                return null;
            }

            var roles = user.UsuarioRoles.Select(ur => ur.Rol.Codigo).ToList();
            var permisos = user.PermisosOperacion.Select(p => new PermisoAreaDto(
                p.IdOperacion,
                p.Operacion.Nombre,
                p.Operacion.Codigo,
                p.PuedeCapturar,
                p.PuedeConsultar,
                p.PuedeGenerarVale
            )).ToList();

            return new UsuarioMeDto(user.IdUsuario, user.Username, user.Nombre, roles, permisos);
        }
    }
}

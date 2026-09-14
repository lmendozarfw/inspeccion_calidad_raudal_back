using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Calidad_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Usuario usuario)
        {
            var jwt = _configuration.GetSection("JwtSettings");
            var secret = jwt["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no configurado");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Username),
                new("FullName", usuario.Nombre)
            };

            foreach (var ur in usuario.UsuarioRoles)
                claims.Add(new Claim(ClaimTypes.Role, ur.Rol.Codigo));

            // Opcional: incluir áreas con permiso de captura como claims
            foreach (var p in usuario.PermisosArea.Where(x => x.PuedeCapturar))
                claims.Add(new Claim("area_captura", p.IdArea.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = int.TryParse(jwt["ExpireMinutes"], out var m) ? m : 480;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = jwt["Issuer"],
                Audience = jwt["Audience"],
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(tokenDescriptor));
        }
    }
}
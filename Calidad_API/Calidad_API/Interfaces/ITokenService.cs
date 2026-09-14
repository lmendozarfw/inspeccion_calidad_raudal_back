using Calidad_API.Models;

namespace Calidad_API.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(Usuario usuario);
    }
}

using Calidad_API.Models;

namespace Calidad_API.Interfaces
{
    public interface IUserRepository
    {
        Task<Usuario?> GetByUsernameAsync(string username);
    }
}

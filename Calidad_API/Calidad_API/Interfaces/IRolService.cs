using Calidad_API.DTOs.Roles;

namespace Calidad_API.Interfaces
{
    public interface IRolService
    {
        Task<IEnumerable<RolDto>> GetAllAsync();
        Task<RolDto?> GetByIdAsync(short id);
    }
}
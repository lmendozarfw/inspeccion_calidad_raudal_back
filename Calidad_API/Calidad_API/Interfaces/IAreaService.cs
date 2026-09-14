using Calidad_API.DTOs.Area;

namespace Calidad_API.Interfaces
{
    public interface IAreaService
    {
        Task<IEnumerable<AreaDto>> GetAllAsync(bool soloActivas = true);
        Task<AreaDto?> GetByIdAsync(long id);
        Task<IEnumerable<AreaDto>> GetByProcesoAsync(string proceso);
        Task<AreaDto> CreateAsync(AreaCreateDto dto);
        Task<AreaDto?> UpdateAsync(long id, AreaUpdateDto dto);
        Task<bool> DeleteAsync(long id); // soft delete
    }
}

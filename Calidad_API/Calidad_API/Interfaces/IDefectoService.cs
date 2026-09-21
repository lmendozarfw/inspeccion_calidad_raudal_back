using Calidad_API.DTOs.Defectos;

namespace Calidad_API.Interfaces
{
    public interface IDefectoService
    {
        Task<IEnumerable<DefectoDto>> GetByAllAsync();
        Task<IEnumerable<DefectoDto>> GetByAreaAsync(long idArea, bool soloActivos = true);
        Task<IEnumerable<DefectoDto>> GetByOperationAndInspectionType(long idOperacion, long idTipoInspeccion);
        Task<DefectoDto?> GetByIdAsync(long id);
        Task<DefectoDto> CreateAsync(DefectoCreateDto dto);
        Task<DefectoDto?> UpdateAsync(long id, DefectoUpdateDto dto);
        Task<bool> DeleteAsync(long id); // soft delete
    }
}

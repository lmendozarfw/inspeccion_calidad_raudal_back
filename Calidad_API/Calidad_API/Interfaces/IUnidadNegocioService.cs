using Calidad_API.DTOs.UnidadesNegocio;

namespace Calidad_API.Interfaces
{
    public interface IUnidadNegocioService
    {
        Task<IEnumerable<UnidadNegocioDto>> GetAllAsync(bool soloActivas = true);
        Task<UnidadNegocioDto?> GetByIdAsync(long id);
        Task<UnidadNegocioDto> CreateAsync(UnidadNegocioCreateDto dto);
        Task<UnidadNegocioDto?> UpdateAsync(long id, UnidadNegocioUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
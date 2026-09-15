using Calidad_API.DTOs.Departamentos;

namespace Calidad_API.Interfaces
{
    public interface IDepartamentoService
    {
        Task<IEnumerable<DepartamentoDto>> GetAllAsync(bool soloActivos = true);
        Task<IEnumerable<DepartamentoDto>> GetByUnidadNegocioAsync(long idUnidadNegocio);
        Task<DepartamentoDto?> GetByIdAsync(long id);
        Task<DepartamentoDto> CreateAsync(DepartamentoCreateDto dto);
        Task<DepartamentoDto?> UpdateAsync(long id, DepartamentoUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
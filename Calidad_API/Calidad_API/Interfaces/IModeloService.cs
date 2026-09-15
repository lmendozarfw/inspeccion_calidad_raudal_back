using Calidad_API.DTOs.Modelos;

namespace Calidad_API.Interfaces
{
    public interface IModeloService
    {
        Task<IEnumerable<ModeloDto>> GetAllAsync(bool soloActivos = true);
        Task<ModeloDto?> GetByIdAsync(long id);
        Task<ModeloDto?> GetByCodigoAsync(string codigo);
        Task<ModeloDto> CreateAsync(ModeloCreateDto dto);
        Task<ModeloDto?> UpdateAsync(long id, ModeloUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
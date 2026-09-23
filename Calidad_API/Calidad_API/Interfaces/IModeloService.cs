using Calidad_API.DTOs.Modelos;

namespace Calidad_API.Interfaces
{
    public interface IModeloService
    {
        Task<IEnumerable<ModeloDto>> GetAllAsync(bool soloActivos = true);
        Task<ModeloDto?> GetByIdAsync(int id);
        Task<ModeloDto?> GetByCodigosAsync(string codigoModeloBase, string codigoCombinacion);
        Task<ModeloDto> CreateAsync(ModeloCreateDto dto);
        Task<ModeloDto?> UpdateAsync(int id, ModeloUpdateDto dto);
        Task<bool> DeleteAsync(int id); // soft: estatus = false
    }
}
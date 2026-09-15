using Calidad_API.DTOs.TiposInspeccion;

namespace Calidad_API.Interfaces
{
    public interface ITipoInspeccionService
    {
        Task<IEnumerable<TipoInspeccionDto>> GetAllAsync(bool soloActivos = true);
        Task<TipoInspeccionDto?> GetByIdAsync(short id);
        Task<TipoInspeccionDto> CreateAsync(TipoInspeccionCreateDto dto);
        Task<TipoInspeccionDto?> UpdateAsync(short id, TipoInspeccionUpdateDto dto);
        Task<bool> DeleteAsync(short id);
    }
}
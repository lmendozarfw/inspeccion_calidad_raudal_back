using Calidad_API.DTOs.Piezas;

namespace Calidad_API.Interfaces
{
    public interface IPiezaService
    {
        Task<IEnumerable<PiezaDto>> GetAllAsync(bool soloActivas = true);
        Task<PiezaDto?> GetByIdAsync(long id);
        Task<PiezaDto> CreateAsync(PiezaCreateDto dto);
        Task<PiezaDto?> UpdateAsync(long id, PiezaUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
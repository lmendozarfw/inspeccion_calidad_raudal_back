using Calidad_API.DTOs.Operaciones;

namespace Calidad_API.Interfaces
{
    public interface IOperacionService
    {
        Task<IEnumerable<OperacionDto>> GetAllAsync(bool soloActivas = true);
        Task<OperacionDto?> GetByIdAsync(long id);
        Task<IEnumerable<OperacionDto>> GetByProcesoAsync(string proceso);
        Task<IEnumerable<OperacionDto>> GetByDepartamentoAsync(long idDepartamento);
        Task<IEnumerable<OperacionPermisoDto>> GetMisPermisosAsync(long idUsuario);
        Task<OperacionDto> CreateAsync(OperacionCreateDto dto);
        Task<OperacionDto?> UpdateAsync(long id, OperacionUpdateDto dto);
        Task<bool> DeleteAsync(long id);
    }
}

using Calidad_API.DTOs.Vales;

namespace Calidad_API.Interfaces
{
    public interface IValeService
    {
        Task<ValeDto?> GetByIdAsync(long id);
        Task<ValeDto?> GetByFolioAsync(string folio);
        Task<IEnumerable<ValeDto>> GetByInspeccionDetalleAsync(long idDetalle);
        Task<ValeDto> GenerarAsync(ValeCreateDto dto, long idUsuario);
        Task<ValeDto?> ActualizarEstadoAsync(long id, ValeUpdateEstadoDto dto);
        Task<IEnumerable<ValeDto>> GetByInspeccionAsync(long idInspeccion);
    }
}


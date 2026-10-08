using Calidad_API.DTOs.Inspecciones;

namespace Calidad_API.Interfaces
{
    public interface IInspeccionService
    {
        Task<InspeccionDto?> GetByIdAsync(long id);
        Task<IEnumerable<InspeccionDto>> GetAll();
        Task<IEnumerable<InspeccionReporteDto>> GetReport(DateTime desde, DateTime hasta);
        Task<IEnumerable<InspeccionPorLoteReporteDto>> GetReportByLotes(DateTime desde, DateTime hasta);
        Task<InpseccionIndicadoresReporteDto> GetReportIndicadores(DateTime desde, DateTime hasta);
        Task<IEnumerable<InspeccionPorOperacionReporteDto>> GetReportePorOpracion(DateTime desde, DateTime hasta);
        Task<IEnumerable<InspeccionDto>> Search(string? search);
        Task<IEnumerable<InspeccionDto>> GetByTransferAsync(long idTransfer);
        Task<List<InspeccionDto>> GetAbiertasByTransferAsync(string programa, string lote, string modelo);
        Task<IEnumerable<InspeccionDto>> GetByAreaAsync(long idArea, DateTime? desde, DateTime? hasta);
        Task<InspeccionDto> CrearAsync(InspeccionCreateDto dto, long idUsuario);
        Task<InspeccionDetalleDto> AgregarDetalleAsync(long idInspeccion, InspeccionDetalleCreateDto dto, long idUsuario);
        Task<InspeccionDto?> CerrarAsync(long idInspeccion, InspeccionCerrarDto dto);
        Task<bool> EliminarDetalleAsync(long idDetalle);
        Task<InspectionRegisterResponse> RegistrarAsync(InspectionRegisterRequest request, long idUsuario);
    }
}

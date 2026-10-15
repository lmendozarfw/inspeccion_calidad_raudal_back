using Calidad_API.DTOs.Dashboard;

namespace Calidad_API.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAll(DashboardDateRange range); // legacy, se puede dejar

    Task<HeatmapOperacionDto> GetHeatmapPorOperacionAsync(DateTime? fecha);
    Task<List<TopDefectosPorOperacionDto>> GetTopPorOperacionDiaAsync(DateTime? fecha);
    Task<DashboardSemanaBloqueDto> GetBloqueRangoAsync(DateTime? desde, DateTime? hasta);

    Task<IReadOnlyList<DashboardDetalleItemDto>> GetDetalleAsync(DashboardDetalleFiltro f);
}
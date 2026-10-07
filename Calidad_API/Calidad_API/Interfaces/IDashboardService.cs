using Calidad_API.DTOs.Dashboard;

namespace Calidad_API.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAll(DashboardDateRange range);

    Task<IReadOnlyList<DashboardDetalleItemDto>> GetDetalleAsync(DashboardDetalleFiltro filtro);
}
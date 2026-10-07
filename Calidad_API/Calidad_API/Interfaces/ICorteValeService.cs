using Calidad_API.DTOs.Vales;

public interface ICorteValeService
{
    /// <summary>Toma PENDIENTE por operación, crea cortes PENDIENTE_AUTH y notifica.</summary>
    Task<CorteEjecutarResponse> EjecutarCorteAsync();

    Task<IEnumerable<CorteResumenDto>> GetPendientesAsync();
    Task<CorteDetalleDto?> GetByIdAsync(long idCorte);  
    Task<ValeDto> AutorizarCorteAsync(long idCorte, string codigoAutorizacion, long idUsuarioAutoriza);
}
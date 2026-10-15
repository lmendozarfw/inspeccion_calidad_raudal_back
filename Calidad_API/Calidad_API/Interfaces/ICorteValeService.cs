using Calidad_API.DTOs.Vales;

public interface ICorteValeService
{
    /// <summary>Toma PENDIENTE por operación, crea cortes PENDIENTE_AUTH y notifica.</summary>
    Task<CorteEjecutarResponse> EjecutarCorteAsync();

    Task<IEnumerable<CorteResumenDto>> GetPendientesAsync(long idUsuario, bool esAdmin);
    Task<CorteDetalleDto?> GetByIdAsync(long idCorte, long idUsuario, bool esAdmin);  
    Task<ValeDto> AutorizarCorteAsync(long idCorte, string codigoAutorizacion, long idUsuarioAutoriza, bool esAdmin);
}
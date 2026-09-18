using Calidad_API.DTOs.CodigoAutorizacion;

namespace Calidad_API.Interfaces;

public interface ICodigoAutorizacionService
{
    public Task<IEnumerable<CodigoAutorizacionDto>> GetCodigosAutorizacionByUsuarioAsync(long idUsuario);
    public Task<CodigoAutorizacionDto?> ObtenerCodigoAutorizacionAsync(long idUsuario);
}
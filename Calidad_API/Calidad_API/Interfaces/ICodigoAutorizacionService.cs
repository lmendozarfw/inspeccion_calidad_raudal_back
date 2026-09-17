using Calidad_API.DTOs.CodigoAutorizacion;

namespace Calidad_API.Interfaces;

public interface ICodigoAutorizacionService
{
    public Task<CodigoAutorizacionDto?> ObtenerCodigoAutorizacionAsync(long IdUsuario);
}
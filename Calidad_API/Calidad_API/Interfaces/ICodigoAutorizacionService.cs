using Calidad_API.DTOs.CodigoAutorizacion;

namespace Calidad_API.Interfaces;
/// <summary>
/// Servicio para manejar códigos de autorización de supervisores.
/// </summary>
public interface ICodigoAutorizacionService
{
    /// <summary>
    /// Obtiene todos los códigos de autorización de un usuario, incluyendo los inactivos.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public Task<IEnumerable<CodigoAutorizacionDto>> GetCodigosAutorizacionByUsuarioAsync(long idUsuario);
    /// <summary>
    /// Obtiene el código de autorización activo de un usuario, si existe.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public Task<CodigoAutorizacionDto?> ObtenerCodigoAutorizacionAsync(long idUsuario);

    /// <summary>
    /// Valida un código en claro. Devuelve el id del supervisor dueño del código, o null si es inválido.
    /// </summary>
    Task<long?> ValidarCodigoAsync(string codigoPlano);
}
using Calidad_API.Data;
using Calidad_API.DTOs.CodigoAutorizacion;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Calidad_API.Models;

namespace Calidad_API.Services;
/// <summary>
/// Servicio para manejar códigos de autorización de supervisores.
/// </summary>
public class CodigoAutorizacionService : ICodigoAutorizacionService
{
    /// <summary>
    /// Contexto de la base de datos para acceder a los códigos de autorización y usuarios.
    /// </summary>
    private readonly ApplicationDbContext _contex;

    /// <summary>
    /// Servicio para cifrar y descifrar los códigos de autorización.
    /// </summary>
    private readonly ICodigoCipher _codigoCipher;

    /// <summary>
    /// Constructor del servicio de códigos de autorización.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="codigoCipher"></param>
    public CodigoAutorizacionService(ApplicationDbContext context, ICodigoCipher codigoCipher)
    {
        _contex = context;
        _codigoCipher = codigoCipher;
    }

    /// <summary>
    /// Obtiene los códigos de autorización asociados a un usuario específico.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task<IEnumerable<CodigoAutorizacionDto>> GetCodigosAutorizacionByUsuarioAsync(long idUsuario)
    {
        var codigos = await _contex.CodigosAutorizacion.AsNoTracking()
            .Where(c => c.IdUsuario == idUsuario)
            .OrderBy(c => c.FechaExpiracion)
            .ToListAsync();

        return codigos.Select(ca => new CodigoAutorizacionDto(
            ca.CodigoCifrado is null ? null : _codigoCipher.Descifrar(ca.CodigoCifrado),
            ca.FechaCreacion,
            ca.FechaExpiracion,
            ca.Activo
        )).ToList();
    }

    /// <summary>
    /// Genera un nuevo código de autorización para un usuario con rol de supervisor.
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task<CodigoAutorizacionDto?> ObtenerCodigoAutorizacionAsync(long idUsuario)
    {
        var tieneRolSupervisor = await _contex.UsuarioRoles.AsNoTracking()
            .AnyAsync(ur => ur.IdUsuario == idUsuario && ur.Rol.Codigo == "SUPERVISOR");
        if (!tieneRolSupervisor)
        {
            return null;
        }

        var codigo = await CrearCodigoUnicoAsync();
        var fechaCreacion = DateTime.UtcNow;
        var fechaExpiracion = fechaCreacion.AddDays(90);
        var entity = new CodigoAutorizacion
        {
            IdUsuario = idUsuario,
            Activo = true,
            CodigoCifrado = _codigoCipher.Cifrar(codigo),
            FechaCreacion = fechaCreacion,
            FechaExpiracion = fechaExpiracion
        };
        _contex.CodigosAutorizacion.Add(entity);
        await _contex.SaveChangesAsync();
        return new CodigoAutorizacionDto(
            codigo, fechaCreacion, fechaExpiracion, true);
    }

    /// <summary>
    /// Genera un código numérico aleatorio de la longitud especificada.
    /// </summary>
    /// <param name="longitud"></param>
    /// <returns></returns>
    private string GenerarCodigo(int longitud)
    {
        var min = (int)Math.Pow(10, longitud - 1);
        var max = (int)Math.Pow(10, longitud);
        return Random.Shared.Next(min, max).ToString();
    }

    /// <summary>
    /// Genera un código único que no esté en uso actualmente por ningún usuario activo.
    /// </summary>
    /// <param name="longitud"></param>
    /// <returns></returns>
    private async Task<string> CrearCodigoUnicoAsync(int longitud = 4)
    {
        var codigosActivos = await _contex.CodigosAutorizacion.AsNoTracking()
            .Where(c => c.Activo && c.CodigoCifrado != null)
            .Select(c => c.CodigoCifrado!)
            .ToListAsync();

        var existentes = codigosActivos
            .Select(cifrado => _codigoCipher.Descifrar(cifrado))
            .ToHashSet();

        while (true)
        {
            var codigo = GenerarCodigo(longitud);
            if (!existentes.Contains(codigo))
            {
                return codigo;
            }
        }
    }

    /// <summary>
    /// Valida un código en claro. Devuelve el id del supervisor dueño del código, o null si es inválido.
    /// </summary>
    /// <param name="codigoPlano"></param>
    /// <returns></returns>
    public async Task<long?> ValidarCodigoAsync(string codigoPlano)
    {
        if (string.IsNullOrWhiteSpace(codigoPlano))
            return null;

        var codigo = codigoPlano.Trim();
        var ahora = DateTime.UtcNow;

        var candidatos = await _contex.CodigosAutorizacion
            .AsNoTracking()
            .Include(c => c.Usuario)
                .ThenInclude(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
            .Where(c => c.Activo
                        && c.CodigoCifrado != null
                        && c.FechaExpiracion >= ahora)
            .ToListAsync();

        foreach (var ca in candidatos)
        {
            string plano;
            try
            {
                plano = _codigoCipher.Descifrar(ca.CodigoCifrado!);
            }
            catch
            {
                continue;
            }

            if (!string.Equals(plano, codigo, StringComparison.Ordinal))
                continue;

            // Solo supervisores (o ADMIN si quieres permitir)
            var esSupervisor = ca.Usuario.UsuarioRoles
                .Any(ur => ur.Rol.Codigo is "SUPERVISOR" or "ADMIN");

            if (!esSupervisor)
                return null;

            return ca.IdUsuario; // quien autoriza
        }

        return null;
    }
}
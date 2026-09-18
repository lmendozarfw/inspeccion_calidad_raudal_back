using Calidad_API.Data;
using Calidad_API.DTOs.CodigoAutorizacion;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Calidad_API.Models;

namespace Calidad_API.Services;

public class CodigoAutorizacionService : ICodigoAutorizacionService
{

    private readonly ApplicationDbContext _contex;
    private readonly ICodigoCipher _codigoCipher;

    public CodigoAutorizacionService(ApplicationDbContext context, ICodigoCipher codigoCipher)
    {
        _contex = context;
        _codigoCipher = codigoCipher;
    }

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

    private string GenerarCodigo(int longitud)
    {
        var min = (int)Math.Pow(10, longitud - 1);
        var max = (int)Math.Pow(10, longitud);
        return Random.Shared.Next(min, max).ToString();
    }

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
}
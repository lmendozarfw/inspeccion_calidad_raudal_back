using Calidad_API.Data;
using Calidad_API.DTOs.CodigoAutorizacion;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Calidad_API.Models;

namespace Calidad_API.Services;

public class CodigoAutorizacionService : ICodigoAutorizacionService
{

    private readonly ApplicationDbContext _contex;

    public CodigoAutorizacionService(ApplicationDbContext context)
    {
        _contex = context;
    }

    public async Task<CodigoAutorizacionDto?> ObtenerCodigoAutorizacionAsync(long IdUsuario)
    {
        var tieneRolSupervisor = await _contex.UsuarioRoles.AsNoTracking()
            .AnyAsync(ur => ur.IdUsuario == IdUsuario && ur.Rol.Codigo == "SUPERVISOR");
        if (!tieneRolSupervisor)
        {
            return null;
        }

        var codigo = await CrearCodigoUnicoAsync();
        var fechaCreacion = DateTime.UtcNow;
        var fechaExpiracion = fechaCreacion.AddDays(90);
        var entity = new CodigoAutorizacion
        {
            IdUsuario = IdUsuario,
            Activo = true,
            CodigoHash = HashCodigo(codigo),
            FechaCreacion = fechaCreacion,
            FechaExpiracion = fechaExpiracion
        };
        _contex.CodigosAutorizacion.Add(entity);
        await _contex.SaveChangesAsync();
        return new CodigoAutorizacionDto(
            codigo, fechaCreacion, fechaExpiracion);
    }

    private string GenerarCodigo(int longitud)
    {
        var min = (int)Math.Pow(10, longitud - 1);
        var max = (int)Math.Pow(10, longitud);
        return Random.Shared.Next(min, max).ToString();
    }

    private string HashCodigo(string codigo)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(codigo)));
    }
    
    private async Task<string> CrearCodigoUnicoAsync(int longitud = 4)
    {
        while (true)
        {
            var codigo = GenerarCodigo(longitud);
            var hash = HashCodigo(codigo);
            var repetido = await _contex.CodigosAutorizacion
                .AsNoTracking()
                .AnyAsync(c => c.CodigoHash == hash && c.Activo);
            if (!repetido) return codigo;
        }
    }
}
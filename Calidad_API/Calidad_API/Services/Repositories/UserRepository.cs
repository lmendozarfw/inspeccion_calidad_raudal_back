using Calidad_API.Data;
using Calidad_API.DTOs.Roles;
using Calidad_API.DTOs.Usuario;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UserRepository(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .Include(u => u.PermisosOperacion)
                    .ThenInclude(p => p.Operacion)
                .FirstOrDefaultAsync(u => u.Username == username && u.Activo);
        }

        public async Task<List<UsuarioDto>> GetAllAsync()
        {
            return await _context.Usuarios.AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    Username = u.Username,
                    NombreCompleto = u.Nombre,
                    IdsRol = u.UsuarioRoles.Select(ur => ur.IdRol).ToList(),
                    Roles = u.UsuarioRoles.Select(ur => new RolDto(ur.Rol.IdRol, ur.Rol.Codigo, ur.Rol.Nombre)).ToList(),
                    IdsOperacion = u.PermisosOperacion.Select(p => p.IdOperacion).ToList(),
                    Activo = u.Activo,
                    FechaCreacion = u.FechaAlta
                })
                .ToListAsync();
        }

        public async Task<UsuarioDto?> GetByIdAsync(long id)
        {
            return await _context.Usuarios.AsNoTracking()
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    Username = u.Username,
                    NombreCompleto = u.Nombre,
                    IdsRol = u.UsuarioRoles.Select(ur => ur.IdRol).ToList(),
                    Roles = u.UsuarioRoles.Select(ur => new RolDto(ur.Rol.IdRol, ur.Rol.Codigo, ur.Rol.Nombre)).ToList(),
                    IdsOperacion = u.PermisosOperacion.Select(p => p.IdOperacion).ToList(),
                    Activo = u.Activo,
                    FechaCreacion = u.FechaAlta
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UsuarioDto?> CreateAsync(CreateUsuarioDto user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.NombreCompleto) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                !user.IdsRol.Any())
            {
                return null;
            }

            if (user.Password.Length < 8)
            {
                return null;
            }

            var usernameNormalizado = user.Username.Trim().ToLower();
            if (await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == usernameNormalizado))
            {
                return null;
            }

            var idsRoles = user.IdsRol.Distinct().ToList();
            if (!await ExistenTodosLosRolesAsync(idsRoles))
            {
                return null;
            }

            var usuario = new Usuario
            {
                Username = user.Username.Trim(),
                Nombre = user.NombreCompleto.Trim(),
                PasswordHash = _passwordHasher.HashPassword(new Usuario(), user.Password),
                Activo = true,
                FechaAlta = DateTime.UtcNow,
                UsuarioRoles = idsRoles.Select(id => new UsuarioRol { IdRol = id }).ToList()
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(usuario.IdUsuario);
        }

        public async Task<UsuarioDto?> UpdateAsync(long id, UpdateUsuarioDto user)
        {
            if (string.IsNullOrWhiteSpace(user.NombreCompleto) || !user.IdsRol.Any())
            {
                return null;
            }

            var idsRoles = user.IdsRol.Distinct().ToList();
            if (!await ExistenTodosLosRolesAsync(idsRoles))
            {
                return null;
            }

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario is null)
            {
                return null;
            }

            var existeUsername = await _context.Usuarios.AnyAsync(u =>
                u.Username.ToLower() == user.Username.Trim().ToLower() && u.IdUsuario != id);
            if (existeUsername)
            {
                return null;
            }

            usuario.Username = user.Username.Trim();
            usuario.Nombre = user.NombreCompleto.Trim();

            _context.UsuarioRoles.RemoveRange(usuario.UsuarioRoles);
            await _context.SaveChangesAsync();

            var nuevosRoles = idsRoles
                .Select(idsRol => new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = idsRol })
                .ToList();
            await _context.UsuarioRoles.AddRangeAsync(nuevosRoles);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(usuario.IdUsuario);
        }

        public async Task<UsuarioDto?> ToggleAsync(long id, ToggleActivoDto toggle)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null)
            {
                return null;
            }

            usuario.Activo = toggle.Activo;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(usuario.IdUsuario);
        }

        public async Task<UsuarioDto?> AsignarOperacionesAsync(long idUsuario, AsignarOperacionesDto dto)
        {
            if (dto.IdsOperacion.Count == 0)
            {
                return null;
            }

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
            if (!usuarioExiste)
            {
                return null;
            }

            var idsOperaciones = dto.IdsOperacion.Distinct().ToList();
            if (!await ExistenTodasLasOperacionesAsync(idsOperaciones))
            {
                return null;
            }

            var permisosActuales = await _context.PermisosOperacion
                .Where(p => p.IdUsuario == idUsuario)
                .ToListAsync();

            _context.PermisosOperacion.RemoveRange(permisosActuales);
            await _context.SaveChangesAsync();

            var nuevosPermisos = idsOperaciones.Select(idOperacion => new PermisoOperacion
            {
                IdUsuario = idUsuario,
                IdOperacion = idOperacion,
                PuedeCapturar = true,
                PuedeConsultar = true,
                PuedeGenerarVale = true
            }).ToList();

            await _context.PermisosOperacion.AddRangeAsync(nuevosPermisos);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(idUsuario);
        }

        private async Task<bool> ExistenTodosLosRolesAsync(List<short> idsRoles)
        {
            var cantidadRolesExistentes = await _context.Roles.CountAsync(r => idsRoles.Contains(r.IdRol));
            return cantidadRolesExistentes == idsRoles.Count;
        }

        private async Task<bool> ExistenTodasLasOperacionesAsync(List<long> idsOperaciones)
        {
            var cantidadOperacionesExistentes = await _context.Operaciones
                .CountAsync(o => idsOperaciones.Contains(o.IdOperacion));
            return cantidadOperacionesExistentes == idsOperaciones.Count;
        }
    }
}
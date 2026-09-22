using Arooaf.Api.Data;
using Arooaf.Api.DTOs.Tramo;
using Arooaf.Api.DTOs.Usuario.CrearUsuario;
using Arooaf.Api.DTOs.Usuario.ActualizarUsuario;
using Arooaf.Api.DTOs.Usuario.ListarUsuarios;
using Arooaf.Api.DTOs.Usuario.ObtenerUsuario;
using Arooaf.Api.Entities;
using Arooaf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UsuariosController(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // GET: api/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListarUsuariosOutput>>> GetUsuarios(
        [FromQuery] string? search = null,
        [FromQuery] UserRole? rol = null,
        [FromQuery] bool? activo = null)
    {
        var query = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Tramos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.NombreCompleto.ToLower().Contains(s) ||
                u.Username.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        if (rol.HasValue)
            query = query.Where(u => u.Rol == rol.Value);

        if (activo.HasValue)
            query = query.Where(u => u.Activo == activo.Value);

        var usuarios = await query
            .OrderByDescending(u => u.FechaCreacion)
            .Select(u => new ListarUsuariosOutput
            {
                Id = u.IdUsuario,
                NombreCompleto = u.NombreCompleto,
                Username = u.Username,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                UltimoAcceso = u.UltimoAcceso,
                Tramos = u.Tramos.Select(t => new TramoDto
                {
                    Id = t.IdTramo,
                    Codigo = t.Codigo,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Activo = t.Activo
                }).ToList()
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // GET: api/usuarios/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObtenerUsuarioOutput>> GetUsuario(Guid id)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Tramos)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario is null)
            return NotFound();

        return Ok(new ObtenerUsuarioOutput
        {
            Id = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Username = usuario.Username,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            UltimoAcceso = usuario.UltimoAcceso,
            Tramos = usuario.Tramos.Select(t => new TramoDto
            {
                Id = t.IdTramo,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                Activo = t.Activo
            }).ToList()
        });
    }

    // POST: api/usuarios
    [HttpPost]
    public async Task<ActionResult<CrearUsuarioOutput>> CreateUsuario([FromBody] CrearUsuarioInput dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { message = "el nombre de usuario ya existe" });

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "el correo ya esta registrado" });

        var tramos = await _db.Tramos
            .Where(t => dto.TramoIds.Contains(t.IdTramo) && t.Activo)
            .ToListAsync();

        if (tramos.Count != dto.TramoIds.Count)
            return BadRequest(new { message = "uno o mas tramos no existen o estan inactivos" });

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Username = dto.Username.Trim().ToLower(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Rol = dto.Rol,
            Activo = true,
            Tramos = tramos
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var response = new CrearUsuarioOutput
        {
            Id = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Username = usuario.Username,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            Tramos = tramos.Select(t => new TramoDto
            {
                Id = t.IdTramo,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                Activo = t.Activo
            }).ToList()
        };

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, response);
    }

    // PUT: api/usuarios/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ActualizarUsuarioOutput>> UpdateUsuario(Guid id, [FromBody] ActualizarUsuarioInput dto)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Tramos)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLower();
            if (await _db.Usuarios.AnyAsync(u => u.Email == email && u.IdUsuario != id))
                return Conflict(new { message = "el correo ya esta registrado por otro usuario" });
            usuario.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(dto.NombreCompleto))
            usuario.NombreCompleto = dto.NombreCompleto.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Password))
            usuario.PasswordHash = _passwordHasher.Hash(dto.Password);

        if (dto.Rol.HasValue)
            usuario.Rol = dto.Rol.Value;

        if (dto.Activo.HasValue)
            usuario.Activo = dto.Activo.Value;

        if (dto.TramoIds != null)
        {
            var tramos = await _db.Tramos
.Where(t => dto.TramoIds.Contains(t.IdTramo) && t.Activo)
                .ToListAsync();

            if (tramos.Count != dto.TramoIds.Count)
                return BadRequest(new { message = "uno o mas tramos no existen o estan inactivos" });

            usuario.Tramos = tramos;
        }

        await _db.SaveChangesAsync();

        return Ok(new ActualizarUsuarioOutput
        {
            Id = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Username = usuario.Username,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            UltimoAcceso = usuario.UltimoAcceso,
            Tramos = usuario.Tramos.Select(t => new TramoDto
            {
                Id = t.IdTramo,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                Activo = t.Activo
            }).ToList()
        });
    }

    // DELETE: api/usuarios/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null)
            return NotFound();

        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(currentUserId, out var currentId) && currentId == id)
            return BadRequest(new { message = "no puedes desactivar tu propia cuenta" });

        usuario.Activo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
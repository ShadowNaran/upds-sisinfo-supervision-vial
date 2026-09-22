using Arooaf.Api.Data;
using Arooaf.Api.DTOs.Auth.Login;
using Arooaf.Api.Entities;
using Arooaf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginOutput>> Login([FromBody] LoginInput request)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

        if (usuario is null)
        {
            return Unauthorized(new { message = "usuario o contrasena incorrectos" });
        }

        if (!usuario.Activo)
        {
            return Unauthorized(new { message = "usuario o contrasena incorrectos" });
        }

        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { message = "usuario o contrasena incorrectos" });
        }

        var token = _tokenService.GenerateToken(usuario, out var expiresAt);

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var result = new LoginResult
        {
            Token = token,
            Rol = usuario.Rol,
            NombreCompleto = usuario.NombreCompleto,
            Username = usuario.Username,
            ExpiraEn = expiresAt,
            ExpiraEnSegundos = Math.Max(1, (long)(expiresAt - DateTime.UtcNow).TotalSeconds)
        };

        var output = new LoginOutput
        {
            Token = result.Token,
            Rol = result.Rol.ToString(),
            Nombre = result.NombreCompleto,
            Username = result.Username,
            ExpiraEn = result.ExpiraEn,
            ExpiraEnSegundos = result.ExpiraEnSegundos
        };

        return Ok(output);
    }
}
using Arooaf.Api.Data;
using Arooaf.Api.DTOs.Auth.Login;
using Arooaf.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Services;

public interface IAuthService
{
    // autentica credenciales devuelve null ante cualquier fallo
    // usuario inexistente contrasena incorrecta o usuario desactivado
    // para no revelar cual de los datos fallo
    Task<LoginResult?> LoginAsync(LoginInput request, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult?> LoginAsync(LoginInput request, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), ct);

        if (usuario is null)
        {
            return null;
        }

        if (!usuario.Activo)
        {
            return null;
        }

        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.GenerateToken(usuario, out var expiresAt);

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new LoginResult
        {
            Token = token,
            Rol = usuario.Rol,
            NombreCompleto = usuario.NombreCompleto,
            Username = usuario.Username,
            ExpiraEn = expiresAt,
            ExpiraEnSegundos = Math.Max(1, (long)(expiresAt - DateTime.UtcNow).TotalSeconds)
        };
    }
}
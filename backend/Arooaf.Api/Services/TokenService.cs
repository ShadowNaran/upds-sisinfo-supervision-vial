using Arooaf.Api.Entities;

namespace Arooaf.Api.Services;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 480;
}

public interface ITokenService
{
    string GenerateToken(Usuario usuario, out DateTime expiresAt);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public string GenerateToken(Usuario usuario, out DateTime expiresAt)
    {
        var key = System.Text.Encoding.UTF8.GetBytes(_settings.SecretKey);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        expiresAt = expires;

        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, usuario.Username),
            new("nombre", usuario.NombreCompleto),
            new(System.Security.Claims.ClaimTypes.Email, usuario.Email),
            new(System.Security.Claims.ClaimTypes.Role, usuario.Rol.ToString())
        };

        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expires,
            NotBefore = DateTime.UtcNow,
            SigningCredentials = credentials
        };

        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}
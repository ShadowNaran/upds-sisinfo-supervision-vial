using Arooaf.Api.Entities;

namespace Arooaf.Api.DTOs.Auth.Login;

public class LoginResult
{
    public required string Token { get; set; }

    public required UserRole Rol { get; set; }

    public required string NombreCompleto { get; set; }

    public required string Username { get; set; }

    public required DateTime ExpiraEn { get; set; }

    public long ExpiraEnSegundos { get; set; }
}
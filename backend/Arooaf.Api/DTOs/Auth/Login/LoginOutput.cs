namespace Arooaf.Api.DTOs.Auth.Login;

public class LoginOutput
{
    public string Token { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }

    public long ExpiraEnSegundos { get; set; }
}
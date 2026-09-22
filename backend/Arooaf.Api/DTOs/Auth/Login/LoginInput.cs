using System.ComponentModel.DataAnnotations;

namespace Arooaf.Api.DTOs.Auth.Login;

public class LoginInput
{
    [Required(ErrorMessage = "el usuario es obligatorio")]
    [MinLength(3, ErrorMessage = "el usuario debe tener al menos 3 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "la contrasena es obligatoria")]
    [MinLength(6, ErrorMessage = "la contrasena debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}
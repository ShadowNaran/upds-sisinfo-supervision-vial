using System.ComponentModel.DataAnnotations;
using Arooaf.Api.Entities;

namespace Arooaf.Api.DTOs.Usuario.ActualizarUsuario;

public class ActualizarUsuarioInput
{
    [MaxLength(200, ErrorMessage = "el nombre completo no puede exceder 200 caracteres")]
    public string? NombreCompleto { get; set; }

    [EmailAddress(ErrorMessage = "el formato del correo no es valido")]
    [MaxLength(150, ErrorMessage = "el correo no puede exceder 150 caracteres")]
    public string? Email { get; set; }

    [MinLength(6, ErrorMessage = "la contrasena debe tener al menos 6 caracteres")]
    [MaxLength(100, ErrorMessage = "la contrasena no puede exceder 100 caracteres")]
    public string? Password { get; set; }

    public UserRole? Rol { get; set; }

    public bool? Activo { get; set; }

    public List<Guid>? TramoIds { get; set; }
}
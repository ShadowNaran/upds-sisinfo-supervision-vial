using System.ComponentModel.DataAnnotations;
using Arooaf.Api.Entities;

namespace Arooaf.Api.DTOs.Usuario.CrearUsuario;

public class CrearUsuarioInput
{
    [Required(ErrorMessage = "el nombre completo es obligatorio")]
    [MaxLength(200, ErrorMessage = "el nombre completo no puede exceder 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "el usuario es obligatorio")]
    [MinLength(3, ErrorMessage = "el usuario debe tener al menos 3 caracteres")]
    [MaxLength(50, ErrorMessage = "el usuario no puede exceder 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "el correo es obligatorio")]
    [EmailAddress(ErrorMessage = "el formato del correo no es valido")]
    [MaxLength(150, ErrorMessage = "el correo no puede exceder 150 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "la contrasena es obligatoria")]
    [MinLength(6, ErrorMessage = "la contrasena debe tener al menos 6 caracteres")]
    [MaxLength(100, ErrorMessage = "la contrasena no puede exceder 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "el rol es obligatorio")]
    public UserRole Rol { get; set; }

    [Required(ErrorMessage = "debe asignar al menos un tramo")]
    [MinLength(1, ErrorMessage = "debe asignar al menos un tramo")]
    public List<Guid> TramoIds { get; set; } = new();
}
using Arooaf.Api.Entities;
using Arooaf.Api.DTOs.Tramo;

namespace Arooaf.Api.DTOs.Usuario.ActualizarUsuario;

public class ActualizarUsuarioOutput
{
    public Guid Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Rol { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public List<TramoDto> Tramos { get; set; } = new();
}
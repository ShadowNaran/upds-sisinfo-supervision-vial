namespace Arooaf.Api.Entities;

public class Usuario
{
    public Guid IdUsuario { get; set; } = Guid.NewGuid();

    public required string NombreCompleto { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Rol { get; set; } = UserRole.SupervisorCampo;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoAcceso { get; set; }

    public ICollection<Tramo> Tramos { get; set; } = new List<Tramo>();
}
namespace Arooaf.Api.Entities;

public class Tramo
{
    public Guid IdTramo { get; set; } = Guid.NewGuid();

    public required string Codigo { get; set; }

    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
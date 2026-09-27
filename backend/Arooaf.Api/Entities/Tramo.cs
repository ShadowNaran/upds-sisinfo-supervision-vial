namespace Arooaf.Api.Entities;

public class Tramo
{
    public Guid IdTramo { get; set; } = Guid.NewGuid();

    public required string Codigo { get; set; }

    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public double LatitudInicio { get; set; }
    public double LongitudInicio { get; set; }
    public double LatitudFin { get; set; }
    public double LongitudFin { get; set; }
    public int KmInicio { get; set; }
    public int KmFin { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public ICollection<Personal> Personal { get; set; } = new List<Personal>();

    public ICollection<Planilla> Planillas { get; set; } = new List<Planilla>();
}
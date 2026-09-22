namespace Arooaf.Api.Entities;

public class Personal
{
    public Guid IdPersonal { get; set; } = Guid.NewGuid();
    public required string NombreCompleto { get; set; }
    public required string Documento { get; set; }
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public Guid IdTramo { get; set; }
    public Tramo? Tramo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public ICollection<PlanillaDetalle> PlanillaDetalles { get; set; } = new List<PlanillaDetalle>();
}
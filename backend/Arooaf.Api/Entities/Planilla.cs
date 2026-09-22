namespace Arooaf.Api.Entities;

public class Planilla
{
    public Guid IdPlanilla { get; set; } = Guid.NewGuid();
    public Guid IdTramo { get; set; }
    public Tramo? Tramo { get; set; }
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public Guid IdResponsable { get; set; }
    public EstadoPlanilla Estado { get; set; } = EstadoPlanilla.Borrador;
    public string? Observaciones { get; set; }
    public string? FirmaBase64 { get; set; }
    public DateTime CreadaEn { get; set; } = DateTime.UtcNow;
    public DateTime? CerradaEn { get; set; }
    public ICollection<PlanillaDetalle> Detalles { get; set; } = new List<PlanillaDetalle>();
}
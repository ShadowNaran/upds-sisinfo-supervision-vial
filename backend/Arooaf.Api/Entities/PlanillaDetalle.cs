namespace Arooaf.Api.Entities;

public class PlanillaDetalle
{
    public Guid IdDetalle { get; set; } = Guid.NewGuid();
    public Guid IdPlanilla { get; set; }
    public Planilla? Planilla { get; set; }
    public Guid IdPersonal { get; set; }
    public Personal? Personal { get; set; }
    public EstadoAsistencia Estado { get; set; } = EstadoAsistencia.NoDisponible;
    public string? Observacion { get; set; }
    public string? Clasificacion { get; set; }
    public string? FotoBase64 { get; set; }
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
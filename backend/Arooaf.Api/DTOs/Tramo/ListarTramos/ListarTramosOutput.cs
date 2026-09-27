namespace Arooaf.Api.DTOs.Tramo.ListarTramos;

public class ListarTramosOutput
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public double LatitudInicio { get; set; }
    public double LongitudInicio { get; set; }
    public double LatitudFin { get; set; }
    public double LongitudFin { get; set; }
    public int KmInicio { get; set; }
    public int KmFin { get; set; }
    public bool Activo { get; set; }
}
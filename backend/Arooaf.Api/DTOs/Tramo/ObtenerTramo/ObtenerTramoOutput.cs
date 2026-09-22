namespace Arooaf.Api.DTOs.Tramo.ObtenerTramo;

public class ObtenerTramoOutput
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }
}
namespace Arooaf.Api.DTOs.Tramo.ActualizarTramo;

public class ActualizarTramoOutput
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }
}
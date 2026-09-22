using System.ComponentModel.DataAnnotations;

namespace Arooaf.Api.DTOs.Tramo.ActualizarTramo;

public class ActualizarTramoInput
{
    [MaxLength(200, ErrorMessage = "el nombre no puede exceder 200 caracteres")]
    public string? Nombre { get; set; }

    [MaxLength(500, ErrorMessage = "la descripcion no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    public bool? Activo { get; set; }
}
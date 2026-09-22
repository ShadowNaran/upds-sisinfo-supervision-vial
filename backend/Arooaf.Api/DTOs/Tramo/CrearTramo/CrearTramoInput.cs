using System.ComponentModel.DataAnnotations;

namespace Arooaf.Api.DTOs.Tramo.CrearTramo;

public class CrearTramoInput
{
    [Required(ErrorMessage = "el codigo es obligatorio")]
    [MaxLength(20, ErrorMessage = "el codigo no puede exceder 20 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "el nombre es obligatorio")]
    [MaxLength(200, ErrorMessage = "el nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "la descripcion no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
}
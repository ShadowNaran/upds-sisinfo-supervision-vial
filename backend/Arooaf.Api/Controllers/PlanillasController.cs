using Arooaf.Api.Data;
using Arooaf.Api.Entities;
using Arooaf.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Controllers;

[ApiController]
[Route("api/planillas")]
[Authorize(Roles = "Administrador,SupervisorCampo,PersonalMicroempresa")]
public class PlanillasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<AlertasHub> _hubContext;

    public PlanillasController(AppDbContext db, IHubContext<AlertasHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanillaResumen>>> Listar(CancellationToken ct)
    {
        var items = await _db.Planillas.AsNoTracking().Include(p => p.Tramo).OrderByDescending(p => p.Fecha)
            .Select(p => new PlanillaResumen(p.IdPlanilla, p.Fecha, p.IdTramo, p.Tramo!.Nombre, p.Estado, p.Detalles.Count, p.Detalles.Count(d => d.Estado == EstadoAsistencia.Presente), p.CerradaEn))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanillaDetalleOutput>> Obtener(Guid id, CancellationToken ct)
    {
        var planilla = await _db.Planillas.AsNoTracking().Include(p => p.Tramo).Include(p => p.Detalles).ThenInclude(d => d.Personal).FirstOrDefaultAsync(p => p.IdPlanilla == id, ct);
        if (planilla is null) return NotFound();
        return Ok(ToOutput(planilla));
    }

    [HttpPost]
    public async Task<ActionResult<PlanillaDetalleOutput>> Crear([FromBody] CrearPlanillaInput input, CancellationToken ct)
    {
        if (!await _db.Tramos.AnyAsync(t => t.IdTramo == input.IdTramo && t.Activo, ct)) return BadRequest(new { message = "el tramo no existe o esta inactivo" });
        var existente = await _db.Planillas.FirstOrDefaultAsync(p => p.IdTramo == input.IdTramo && p.Fecha == input.Fecha, ct);
        if (existente != null) return Ok(await ObtenerSalida(existente.IdPlanilla, ct));
        var existentes = await _db.Personal.Where(p => p.IdTramo == input.IdTramo && p.Activo).ToListAsync(ct);
        var planilla = new Planilla { IdTramo = input.IdTramo, IdResponsable = UsuarioActual(), Fecha = input.Fecha, Observaciones = input.Observaciones };
        planilla.Detalles = existentes.Select(p => new PlanillaDetalle { IdPersonal = p.IdPersonal, Estado = EstadoAsistencia.NoDisponible }).ToList();
        _db.Planillas.Add(planilla);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = planilla.IdPlanilla }, await ObtenerSalida(planilla.IdPlanilla, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlanillaDetalleOutput>> Actualizar(Guid id, [FromBody] ActualizarPlanillaInput input, CancellationToken ct)
    {
        var planilla = await _db.Planillas.Include(p => p.Tramo).Include(p => p.Detalles).FirstOrDefaultAsync(p => p.IdPlanilla == id, ct);
        if (planilla is null) return NotFound();
        if (planilla.Estado == EstadoPlanilla.Cerrada) return Conflict(new { message = "la planilla ya esta cerrada" });

        if (planilla.TimestampLocal.HasValue && input.TimestampLocal.HasValue && input.TimestampLocal.Value < planilla.TimestampLocal.Value)
        {
            // ignora cambios locales anteriores al ultimo registro del servidor
            return Ok(await ObtenerSalida(id, ct));
        }
        
        if (!input.Detalles.Any(d => !string.IsNullOrWhiteSpace(d.FotoBase64))) return BadRequest(new { message = "se requiere al menos una fotografía como evidencia para guardar el reporte" });
        foreach (var detalle in input.Detalles)
        {
            var actual = planilla.Detalles.FirstOrDefault(d => d.IdDetalle == detalle.IdDetalle);
            if (actual is null) continue;
            actual.Estado = detalle.Estado;
            actual.Observacion = detalle.Observacion;
            actual.Clasificacion = detalle.Clasificacion;
            actual.FotoBase64 = detalle.FotoBase64;
            actual.Latitud = detalle.Latitud;
            actual.Longitud = detalle.Longitud;
            actual.Kilometraje = detalle.Kilometraje;
            actual.Severidad = detalle.Severidad;
            actual.ActualizadoEn = DateTime.UtcNow;
        }
        planilla.Observaciones = input.Observaciones;
        if (input.TimestampLocal.HasValue) planilla.TimestampLocal = input.TimestampLocal.Value;
        
        await _db.SaveChangesAsync(ct);

        // envia una alerta si el tramo no es transitable
        foreach (var detalle in input.Detalles)
        {
            if (detalle.Clasificacion == "No transita" || detalle.Severidad == NivelSeveridad.NoTransitable)
            {
                var alerta = new AlertaCriticaMsg(
                    planilla.Tramo?.Nombre ?? "Tramo desconocido",
                    detalle.Kilometraje,
                    DateTime.UtcNow.ToString("HH:mm"),
                    detalle.Clasificacion ?? "No transita",
                    detalle.FotoBase64
                );
                await _hubContext.Clients.All.SendAsync("RecibirAlertaCritica", alerta, ct);
            }
        }

        return Ok(await ObtenerSalida(id, ct));
    }

    [HttpPost("{id:guid}/cerrar")]
    public async Task<ActionResult<PlanillaDetalleOutput>> Cerrar(Guid id, CancellationToken ct)
    {
        var planilla = await _db.Planillas.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.IdPlanilla == id, ct);
        if (planilla is null) return NotFound();
        if (planilla.Detalles.Count == 0) return BadRequest(new { message = "la planilla no tiene personal" });
        planilla.Estado = EstadoPlanilla.Cerrada;
        planilla.CerradaEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(await ObtenerSalida(id, ct));
    }

    [HttpPost("detalle/{idDetalle:guid}/mitigacion")]
    public async Task<IActionResult> RegistrarMitigacion(Guid idDetalle, [FromBody] MitigacionInput input, CancellationToken ct)
    {
        var detalle = await _db.Set<PlanillaDetalle>().FindAsync([idDetalle], ct);
        if (detalle is null) return NotFound();
        if (string.IsNullOrWhiteSpace(input.AccionMitigacion)) return BadRequest(new { message = "La observación es obligatoria" });

        detalle.AccionMitigacion = input.AccionMitigacion;
        detalle.EsFalsoPositivo = input.EsFalsoPositivo;
        detalle.HoraMitigacion = DateTime.UtcNow;
        
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/firmar")]
    public async Task<IActionResult> Firmar(Guid id, [FromBody] FirmaInput input, CancellationToken ct)
    {
        var planilla = await _db.Planillas.FindAsync([id], ct);
        if (planilla is null) return NotFound();
        if (string.IsNullOrWhiteSpace(input.FirmaBase64)) return BadRequest(new { message = "la firma es obligatoria" });
        planilla.FirmaBase64 = input.FirmaBase64;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Guid UsuarioActual() => Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

    private async Task<PlanillaDetalleOutput?> ObtenerSalida(Guid id, CancellationToken ct)
    {
        var p = await _db.Planillas.AsNoTracking().Include(x => x.Tramo).Include(x => x.Detalles).ThenInclude(x => x.Personal).FirstOrDefaultAsync(x => x.IdPlanilla == id, ct);
        return p is null ? null : ToOutput(p);
    }

    private static PlanillaDetalleOutput ToOutput(Planilla p) => new(p.IdPlanilla, p.Fecha, p.IdTramo, p.Tramo?.Nombre ?? string.Empty, p.Estado, p.Observaciones, p.FirmaBase64 is not null, p.FirmaBase64, p.Detalles.Select(d => new DetalleOutput(d.IdDetalle, d.IdPersonal, d.Personal?.NombreCompleto ?? string.Empty, d.Estado, d.Observacion, d.Clasificacion, d.FotoBase64, d.Latitud, d.Longitud, d.Kilometraje, d.AccionMitigacion, d.HoraMitigacion, d.EsFalsoPositivo, d.Severidad)).ToList());

    public sealed record CrearPlanillaInput(Guid IdTramo, DateOnly Fecha, string? Observaciones);
    public sealed record ActualizarPlanillaInput(string? Observaciones, List<DetalleInput> Detalles, long? TimestampLocal);
    public sealed record DetalleInput(Guid IdDetalle, EstadoAsistencia Estado, string? Observacion, string? Clasificacion, string? FotoBase64, double? Latitud, double? Longitud, double? Kilometraje, NivelSeveridad? Severidad);
    public sealed record FirmaInput(string FirmaBase64);
    public sealed record MitigacionInput(string AccionMitigacion, bool EsFalsoPositivo = false);
    public sealed record PlanillaResumen(Guid Id, DateOnly Fecha, Guid IdTramo, string Tramo, EstadoPlanilla Estado, int Personal, int Presentes, DateTime? CerradaEn);
    public sealed record PlanillaDetalleOutput(Guid Id, DateOnly Fecha, Guid IdTramo, string Tramo, EstadoPlanilla Estado, string? Observaciones, bool TieneFirma, string? FirmaBase64, List<DetalleOutput> Detalles);
    public sealed record DetalleOutput(Guid Id, Guid IdPersonal, string Personal, EstadoAsistencia Estado, string? Observacion, string? Clasificacion, string? FotoBase64, double? Latitud, double? Longitud, double? Kilometraje, string? AccionMitigacion, DateTime? HoraMitigacion, bool EsFalsoPositivo, NivelSeveridad? Severidad);
}
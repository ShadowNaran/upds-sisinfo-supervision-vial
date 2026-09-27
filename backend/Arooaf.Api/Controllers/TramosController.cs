using Arooaf.Api.Data;
using Arooaf.Api.DTOs.Tramo.CrearTramo;
using Arooaf.Api.DTOs.Tramo.ActualizarTramo;
using Arooaf.Api.DTOs.Tramo.ListarTramos;
using Arooaf.Api.DTOs.Tramo.ObtenerTramo;
using Arooaf.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,SupervisorCampo,PersonalMicroempresa")]
public class TramosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TramosController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/tramos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListarTramosOutput>>> GetTramos(
        [FromQuery] bool? activo = null)
    {
        var query = _db.Tramos.AsNoTracking().AsQueryable();

        if (!User.IsInRole("Administrador"))
            query = query.Where(t => t.Activo);

        if (activo.HasValue)
            query = query.Where(t => t.Activo == activo.Value);

        var tramos = await query
            .OrderBy(t => t.Codigo)
            .Select(t => new ListarTramosOutput
            {
                Id = t.IdTramo,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                LatitudInicio = t.LatitudInicio,
                LongitudInicio = t.LongitudInicio,
                LatitudFin = t.LatitudFin,
                LongitudFin = t.LongitudFin,
                KmInicio = t.KmInicio,
                KmFin = t.KmFin,
                Activo = t.Activo
            })
            .ToListAsync();

        return Ok(tramos);
    }

    // GET: api/tramos/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObtenerTramoOutput>> GetTramo(Guid id)
    {
        var tramo = await _db.Tramos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTramo == id);

        if (tramo is null)
            return NotFound();

        return Ok(new ObtenerTramoOutput
        {
            Id = tramo.IdTramo,
            Codigo = tramo.Codigo,
            Nombre = tramo.Nombre,
            Descripcion = tramo.Descripcion,
            KmInicio = tramo.KmInicio,
            KmFin = tramo.KmFin,
            Activo = tramo.Activo
        });
    }

    // POST: api/tramos
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<CrearTramoOutput>> CreateTramo([FromBody] CrearTramoInput dto)
    {
        if (dto.KmInicio > dto.KmFin)
            return BadRequest(new { message = "el kilometro inicial no puede ser mayor al kilometro final" });

        if (await _db.Tramos.AnyAsync(t => t.Codigo == dto.Codigo))
            return Conflict(new { message = "el codigo de tramo ya existe" });

        var tramo = new Tramo
        {
            Codigo = dto.Codigo.Trim().ToUpper(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            KmInicio = dto.KmInicio,
            KmFin = dto.KmFin,
            Activo = true
        };

        _db.Tramos.Add(tramo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTramo), new { id = tramo.IdTramo }, new CrearTramoOutput
        {
            Id = tramo.IdTramo,
            Codigo = tramo.Codigo,
            Nombre = tramo.Nombre,
            Descripcion = tramo.Descripcion,
            KmInicio = tramo.KmInicio,
            KmFin = tramo.KmFin,
            Activo = tramo.Activo
        });
    }

    // PUT: api/tramos/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateTramo(Guid id, [FromBody] ActualizarTramoInput dto)
    {
        var tramo = await _db.Tramos.FindAsync(id);
        if (tramo is null)
            return NotFound();

        var kmInicio = dto.KmInicio ?? tramo.KmInicio;
        var kmFin = dto.KmFin ?? tramo.KmFin;
        if (kmInicio > kmFin)
            return BadRequest(new { message = "el kilometro inicial no puede ser mayor al kilometro final" });

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            tramo.Nombre = dto.Nombre.Trim();

        if (dto.Descripcion != null)
            tramo.Descripcion = dto.Descripcion.Trim();

        if (dto.KmInicio.HasValue)
            tramo.KmInicio = dto.KmInicio.Value;

        if (dto.KmFin.HasValue)
            tramo.KmFin = dto.KmFin.Value;

        if (dto.Activo.HasValue)
            tramo.Activo = dto.Activo.Value;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/tramos/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteTramo(Guid id)
    {
        var tramo = await _db.Tramos.FindAsync(id);
        if (tramo is null)
            return NotFound();

        tramo.Activo = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
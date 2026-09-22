using Arooaf.Api.Data;
using Arooaf.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Controllers;

[ApiController]
[Route("api/personal")]
[Authorize(Roles = "Administrador,SupervisorCampo")]
public class PersonalController : ControllerBase
{
    private readonly AppDbContext _db;

    public PersonalController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonalOutput>>> Listar([FromQuery] Guid? tramoId, CancellationToken ct)
    {
        var query = _db.Personal.AsNoTracking().Where(p => p.Activo);
        if (tramoId.HasValue) query = query.Where(p => p.IdTramo == tramoId.Value);

        return Ok(await query.OrderBy(p => p.NombreCompleto).Select(p => new PersonalOutput(
            p.IdPersonal, p.NombreCompleto, p.Documento, p.Cargo, p.Telefono, p.IdTramo, p.Tramo!.Nombre)).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<PersonalOutput>> Crear([FromBody] PersonalInput input, CancellationToken ct)
    {
        if (!await _db.Tramos.AnyAsync(t => t.IdTramo == input.IdTramo && t.Activo, ct))
            return BadRequest(new { message = "el tramo no existe o esta inactivo" });

        var existe = await _db.Personal.AnyAsync(p => p.Documento == input.Documento && p.IdTramo == input.IdTramo, ct);
        if (existe) return Conflict(new { message = "el documento ya existe en ese tramo" });

        var personal = new Personal
        {
            NombreCompleto = input.NombreCompleto.Trim(),
            Documento = input.Documento.Trim(),
            Cargo = input.Cargo?.Trim(),
            Telefono = input.Telefono?.Trim(),
            IdTramo = input.IdTramo
        };
        _db.Personal.Add(personal);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(personal).Reference(p => p.Tramo).LoadAsync(ct);
        return CreatedAtAction(nameof(Listar), new { id = personal.IdPersonal }, ToOutput(personal));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var personal = await _db.Personal.FindAsync([id], ct);
        if (personal is null) return NotFound();
        personal.Activo = false;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static PersonalOutput ToOutput(Personal p) => new(p.IdPersonal, p.NombreCompleto, p.Documento, p.Cargo, p.Telefono, p.IdTramo, p.Tramo?.Nombre ?? string.Empty);

    public sealed record PersonalInput(string NombreCompleto, string Documento, string? Cargo, string? Telefono, Guid IdTramo);
    public sealed record PersonalOutput(Guid Id, string NombreCompleto, string Documento, string? Cargo, string? Telefono, Guid IdTramo, string Tramo);
}
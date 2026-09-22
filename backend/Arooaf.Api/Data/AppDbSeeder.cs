using Arooaf.Api.Entities;

namespace Arooaf.Api.Data;

public static class AppDbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Usuarios.Any())
        {
            return;
        }

        // crear tramos base
        var tramos = new[]
        {
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-01",
                Nombre = "Tramo 1: Carretera Principal - Km 0 a 50",
                Descripcion = "Tramo principal de la carretera nacional",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-02",
                Nombre = "Tramo 2: Ruta Alterna - Km 50 a 100",
                Descripcion = "Ruta alterna por la zona norte",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-03",
                Nombre = "Tramo 3: Acceso Sur - Km 100 a 150",
                Descripcion = "Acceso sur hacia la zona industrial",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            }
        };

        db.Tramos.AddRange(tramos);
        db.SaveChanges();

        // obtener ids de tramos para asignar
        var tramo1 = tramos[0].IdTramo;
        var tramo2 = tramos[1].IdTramo;
        var tramo3 = tramos[2].IdTramo;

        var usuarios = new[]
        {
            new Usuario
            {
                IdUsuario = Guid.NewGuid(),
                NombreCompleto = "Administración Central AROOMAF",
                Username = "admin",
                Email = "admin@aroomaf.bo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin.123!"),
                Rol = UserRole.Administrador,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                Tramos = new List<Tramo> { tramos[0], tramos[1], tramos[2] } // admin ve todos
            },
            new Usuario
            {
                IdUsuario = Guid.NewGuid(),
                NombreCompleto = "Ing. Carlos Ríos López",
                Username = "supervisor",
                Email = "supervisor@aroomaf.bo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sup.123!"),
                Rol = UserRole.SupervisorCampo,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                Tramos = new List<Tramo> { tramos[0], tramos[1] } // supervisor ve tramo 1 y 2
            },
            new Usuario
            {
                IdUsuario = Guid.NewGuid(),
                NombreCompleto = "Personal de Microempresa",
                Username = "personal",
                Email = "personal@aroomaf.bo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Per.123!"),
                Rol = UserRole.PersonalMicroempresa,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                Tramos = new List<Tramo> { tramos[2] } // personal ve solo tramo 3
            }
        };

        db.Usuarios.AddRange(usuarios);
        db.SaveChanges();
    }
}
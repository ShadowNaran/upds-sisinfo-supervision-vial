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

        // tramos iniciales
        var tramos = new[]
        {
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-01",
                Nombre = "Tramo 1: Carretera Principal - Km 0 a 50",
                Descripcion = "Tramo principal de la carretera nacional",
                LatitudInicio = -16.5000, LongitudInicio = -68.1500, // la paz
                LatitudFin = -16.8000, LongitudFin = -68.4500, // laja
                KmInicio = 0, KmFin = 50,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-02",
                Nombre = "Tramo 2: Ruta Alterna - Km 50 a 100",
                Descripcion = "Ruta alterna por la zona norte",
                LatitudInicio = -16.8000, LongitudInicio = -68.4500,
                LatitudFin = -17.1000, LongitudFin = -68.7500,
                KmInicio = 50, KmFin = 100,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Tramo
            {
                IdTramo = Guid.NewGuid(),
                Codigo = "TR-03",
                Nombre = "Tramo 3: Acceso Sur - Km 100 a 150",
                Descripcion = "Acceso sur hacia la zona industrial",
                LatitudInicio = -17.1000, LongitudInicio = -68.7500,
                LatitudFin = -17.4000, LongitudFin = -69.0500,
                KmInicio = 100, KmFin = 150,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            }
        };

        db.Tramos.AddRange(tramos);
        db.SaveChanges();

        // asigna tramos a cada usuario
        var tramo1 = tramos[0].IdTramo;
        var tramo2 = tramos[1].IdTramo;
        var tramo3 = tramos[2].IdTramo;

        db.Personal.AddRange(
            new Personal { NombreCompleto = "María López Quispe", Documento = "CI-1001", Cargo = "Cuadrilla vial", IdTramo = tramo1 },
            new Personal { NombreCompleto = "José Mamani Rojas", Documento = "CI-1002", Cargo = "Operador", IdTramo = tramo1 },
            new Personal { NombreCompleto = "Ana Flores Vargas", Documento = "CI-2001", Cargo = "Ayudante", IdTramo = tramo2 },
            new Personal { NombreCompleto = "Luis Condori Nina", Documento = "CI-3001", Cargo = "Señalización", IdTramo = tramo3 });
        db.SaveChanges();

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
                Tramos = new List<Tramo> { tramos[0], tramos[1], tramos[2] } // acceso a todos los tramos
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
                Tramos = new List<Tramo> { tramos[0], tramos[1] } // acceso a dos tramos
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
                Tramos = new List<Tramo> { tramos[2] } // acceso a un tramo
            }
        };

        db.Usuarios.AddRange(usuarios);
        db.SaveChanges();
    }
}
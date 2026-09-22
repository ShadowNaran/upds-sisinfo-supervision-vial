using Arooaf.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arooaf.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tramo> Tramos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // claves primarias
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
        modelBuilder.Entity<Tramo>().HasKey(t => t.IdTramo);

        // configuracion de usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.Property(u => u.NombreCompleto)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.Rol)
                .HasConversion<int>()
                .IsRequired();

            entity.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_username");

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_email");
        });

        // configuracion de tramo
        modelBuilder.Entity<Tramo>(entity =>
        {
            entity.ToTable("tramos");

            entity.Property(t => t.Codigo)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(t => t.Nombre)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(t => t.Descripcion)
                .HasMaxLength(500);

            entity.HasIndex(t => t.Codigo)
                .IsUnique()
                .HasDatabaseName("ix_tramos_codigo");
        });

        // relacion muchos a muchos usuario tramo
        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Tramos)
            .WithMany(t => t.Usuarios)
            .UsingEntity<Dictionary<string, object>>(
                "usuario_tramo",
                j => j.HasOne<Tramo>().WithMany().HasForeignKey("id_tramo").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Usuario>().WithMany().HasForeignKey("id_usuario").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("id_usuario", "id_tramo");
                    j.ToTable("usuario_tramo");
                });
    }
}
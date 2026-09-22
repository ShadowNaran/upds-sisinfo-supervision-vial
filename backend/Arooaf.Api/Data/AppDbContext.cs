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
    public DbSet<Personal> Personal { get; set; }
    public DbSet<Planilla> Planillas { get; set; }
    public DbSet<PlanillaDetalle> PlanillaDetalles { get; set; }

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

        modelBuilder.Entity<Personal>(entity =>
        {
            entity.ToTable("personal");
            entity.HasKey(p => p.IdPersonal);
            entity.Property(p => p.NombreCompleto).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Documento).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Cargo).HasMaxLength(120);
            entity.Property(p => p.Telefono).HasMaxLength(40);
            entity.HasIndex(p => new { p.Documento, p.IdTramo }).IsUnique();
            entity.HasOne(p => p.Tramo).WithMany(t => t.Personal).HasForeignKey(p => p.IdTramo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Planilla>(entity =>
        {
            entity.ToTable("planillas");
            entity.HasKey(p => p.IdPlanilla);
            entity.Property(p => p.Estado).HasConversion<int>();
            entity.Property(p => p.Observaciones).HasMaxLength(1000);
            entity.Property(p => p.FirmaBase64).HasColumnType("text");
            entity.HasIndex(p => new { p.IdTramo, p.Fecha }).IsUnique();
            entity.HasOne(p => p.Tramo).WithMany(t => t.Planillas).HasForeignKey(p => p.IdTramo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlanillaDetalle>(entity =>
        {
            entity.ToTable("planilla_detalles");
            entity.HasKey(d => d.IdDetalle);
            entity.Property(d => d.Estado).HasConversion<int>();
            entity.Property(d => d.Observacion).HasMaxLength(500);
            entity.Property(d => d.Clasificacion).HasMaxLength(100);
            entity.Property(d => d.FotoBase64).HasColumnType("text");
            entity.HasIndex(d => new { d.IdPlanilla, d.IdPersonal }).IsUnique();
            entity.HasOne(d => d.Planilla).WithMany(p => p.Detalles).HasForeignKey(d => d.IdPlanilla).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Personal).WithMany(p => p.PlanillaDetalles).HasForeignKey(d => d.IdPersonal).OnDelete(DeleteBehavior.Restrict);
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
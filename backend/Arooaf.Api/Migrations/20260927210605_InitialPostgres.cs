using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arooaf.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tramos",
                columns: table => new
                {
                    IdTramo = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LatitudInicio = table.Column<double>(type: "double precision", nullable: false),
                    LongitudInicio = table.Column<double>(type: "double precision", nullable: false),
                    LatitudFin = table.Column<double>(type: "double precision", nullable: false),
                    LongitudFin = table.Column<double>(type: "double precision", nullable: false),
                    KmInicio = table.Column<int>(type: "integer", nullable: false),
                    KmFin = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tramos", x => x.IdTramo);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "personal",
                columns: table => new
                {
                    IdPersonal = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cargo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IdTramo = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstadoValidacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal", x => x.IdPersonal);
                    table.ForeignKey(
                        name: "FK_personal_tramos_IdTramo",
                        column: x => x.IdTramo,
                        principalTable: "tramos",
                        principalColumn: "IdTramo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planillas",
                columns: table => new
                {
                    IdPlanilla = table.Column<Guid>(type: "uuid", nullable: false),
                    IdTramo = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    IdResponsable = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FirmaBase64 = table.Column<string>(type: "text", nullable: true),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimestampLocal = table.Column<long>(type: "bigint", nullable: true),
                    CerradaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planillas", x => x.IdPlanilla);
                    table.ForeignKey(
                        name: "FK_planillas_tramos_IdTramo",
                        column: x => x.IdTramo,
                        principalTable: "tramos",
                        principalColumn: "IdTramo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_tramo",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tramo = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_tramo", x => new { x.id_usuario, x.id_tramo });
                    table.ForeignKey(
                        name: "FK_usuario_tramo_tramos_id_tramo",
                        column: x => x.id_tramo,
                        principalTable: "tramos",
                        principalColumn: "IdTramo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_tramo_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planilla_detalles",
                columns: table => new
                {
                    IdDetalle = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPlanilla = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPersonal = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Clasificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FotoBase64 = table.Column<string>(type: "text", nullable: true),
                    Latitud = table.Column<double>(type: "double precision", nullable: true),
                    Longitud = table.Column<double>(type: "double precision", nullable: true),
                    Kilometraje = table.Column<double>(type: "double precision", nullable: true),
                    AccionMitigacion = table.Column<string>(type: "text", nullable: true),
                    HoraMitigacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EsFalsoPositivo = table.Column<bool>(type: "boolean", nullable: false),
                    Severidad = table.Column<int>(type: "integer", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planilla_detalles", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK_planilla_detalles_personal_IdPersonal",
                        column: x => x.IdPersonal,
                        principalTable: "personal",
                        principalColumn: "IdPersonal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planilla_detalles_planillas_IdPlanilla",
                        column: x => x.IdPlanilla,
                        principalTable: "planillas",
                        principalColumn: "IdPlanilla",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personal_Documento_IdTramo",
                table: "personal",
                columns: new[] { "Documento", "IdTramo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personal_IdTramo",
                table: "personal",
                column: "IdTramo");

            migrationBuilder.CreateIndex(
                name: "IX_planilla_detalles_IdPersonal",
                table: "planilla_detalles",
                column: "IdPersonal");

            migrationBuilder.CreateIndex(
                name: "IX_planilla_detalles_IdPlanilla_IdPersonal",
                table: "planilla_detalles",
                columns: new[] { "IdPlanilla", "IdPersonal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planillas_IdTramo_Fecha",
                table: "planillas",
                columns: new[] { "IdTramo", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tramos_codigo",
                table: "tramos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_tramo_id_tramo",
                table: "usuario_tramo",
                column: "id_tramo");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_username",
                table: "usuarios",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planilla_detalles");

            migrationBuilder.DropTable(
                name: "usuario_tramo");

            migrationBuilder.DropTable(
                name: "personal");

            migrationBuilder.DropTable(
                name: "planillas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "tramos");
        }
    }
}

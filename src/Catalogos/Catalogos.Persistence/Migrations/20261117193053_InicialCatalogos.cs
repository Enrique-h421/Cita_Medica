using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalogos.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    EspecialidadID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.EspecialidadID);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    PacienteID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dui = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.PacienteID);
                });

            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    MedicoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroJVPM = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EspecialidadID = table.Column<int>(type: "int", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.MedicoID);
                    table.ForeignKey(
                        name: "FK_Medicos_Especialidades_EspecialidadID",
                        column: x => x.EspecialidadID,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Especialidades",
                columns: new[] { "EspecialidadID", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Consulta general y control de rutina", "Medicina General" },
                    { 2, true, "Atención de niños y adolescentes", "Pediatría" },
                    { 3, true, "Salud de la mujer", "Ginecología" },
                    { 4, true, "Corazón y sistema circulatorio", "Cardiología" },
                    { 5, true, "Piel, cabello y uñas", "Dermatología" }
                });

            migrationBuilder.InsertData(
                table: "Pacientes",
                columns: new[] { "PacienteID", "Activo", "Apellidos", "Correo", "Direccion", "Dui", "FechaNacimiento", "FechaRegistro", "Nombres", "Sexo", "Telefono" },
                values: new object[,]
                {
                    { 1L, true, "Argueta Flores", "mjargueta@correo.com", "Col. Escalón, San Salvador", "04567891-2", new DateTime(1992, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "María José", "F", "7845-2210" },
                    { 2L, true, "Martínez Lemus", "jcmartinez@correo.com", "Soyapango, San Salvador", "03218765-4", new DateTime(1985, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "Juan Carlos", "M", "7712-9087" },
                    { 3L, true, "Flores Campos", "reflores@correo.com", "Santa Tecla, La Libertad", "05543210-9", new DateTime(1978, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "Rosa Elena", "F", "7033-4512" },
                    { 4L, true, "Ramírez Peña", "laramirez@correo.com", "Mejicanos, San Salvador", "02987654-1", new DateTime(2001, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "Luis Alberto", "M", "7890-1123" },
                    { 5L, true, "Sánchez Mendoza", "clsanchez@correo.com", "San Miguel", "06112233-5", new DateTime(1966, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "Carmen Lidia", "F", "7456-7788" },
                    { 6L, true, "Rivera Call", "omrivera@correo.com", "Santa Ana", "07334455-6", new DateTime(1995, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), "Óscar Mauricio", "M", "6023-9900" }
                });

            migrationBuilder.InsertData(
                table: "Medicos",
                columns: new[] { "MedicoID", "Activo", "Apellidos", "Correo", "EspecialidadID", "Nombres", "NumeroJVPM", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "Menjívar Rivas", "cmenjivar@clinica.sv", 1, "Carlos Alberto", "JVPM-10234", "2250-1100" },
                    { 2, true, "Portillo Guzmán", "aportillo@clinica.sv", 2, "Ana Beatriz", "JVPM-10987", "2250-1101" },
                    { 3, true, "Hernández Mejía", "jhernandez@clinica.sv", 4, "José Ricardo", "JVPM-11455", "2250-1102" },
                    { 4, true, "Cruz Alvarado", "scruz@clinica.sv", 3, "Sofía Elena", "JVPM-11890", "2250-1103" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadID",
                table: "Medicos",
                column: "EspecialidadID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Especialidades");
        }
    }
}

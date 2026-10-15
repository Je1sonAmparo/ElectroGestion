using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class CrearAgendaVisitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hora = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_SolicitudId",
                table: "Visitas",
                column: "SolicitudId");

            migrationBuilder.Sql("""
                INSERT INTO [Visitas] ([SolicitudId], [Fecha], [Hora], [Estado])
                SELECT solicitud.[Id], solicitud.[Fecha], solicitud.[Hora],
                       estado.[Nombre]
                FROM [Solicitudes] solicitud
                INNER JOIN [EstadosSolicitud] estado ON estado.[Id] = solicitud.[EstadoSolicitudId]
                WHERE NULLIF(LTRIM(RTRIM(solicitud.[Hora])), N'') IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Visitas");
        }
    }
}

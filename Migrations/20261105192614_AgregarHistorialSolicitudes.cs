using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesEventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEventos_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEventos_SolicitudId_Fecha",
                table: "SolicitudesEventos",
                columns: new[] { "SolicitudId", "Fecha" });

            migrationBuilder.Sql(@"
INSERT INTO [SolicitudesEventos] ([SolicitudId], [Fecha], [Titulo], [Detalle])
SELECT s.[Id], s.[FechaSolicitud], N'Solicitud registrada', N'Se registró la solicitud del servicio.'
FROM [Solicitudes] s WHERE s.[IsDeleted] = 0
UNION ALL
SELECT c.[SolicitudDbId], c.[Fecha], N'Cotización registrada', CONCAT(N'Cotización ', c.[Estado], N'; total RD$ ', CONVERT(nvarchar(40), c.[MontoTotal]))
FROM [Cotizaciones] c WHERE c.[SolicitudDbId] IS NOT NULL
UNION ALL
SELECT c.[SolicitudDbId], c.[FechaEnvio], N'Cotización enviada', N'La cotización fue enviada al cliente.'
FROM [Cotizaciones] c WHERE c.[SolicitudDbId] IS NOT NULL AND c.[FechaEnvio] IS NOT NULL
UNION ALL
SELECT c.[SolicitudDbId], c.[FechaRespuesta], N'Respuesta a cotización', CONCAT(N'La cotización fue ', c.[Estado], N'.')
FROM [Cotizaciones] c WHERE c.[SolicitudDbId] IS NOT NULL AND c.[FechaRespuesta] IS NOT NULL
UNION ALL
SELECT v.[SolicitudId], v.[Fecha], N'Visita programada', CONCAT(N'Visita ', CONVERT(nvarchar(5), v.[Hora]), N' a ', CONVERT(nvarchar(5), v.[HoraFin]), N'; estado: ', v.[Estado], N'.')
FROM [Visitas] v WHERE v.[IsDeleted] = 0
UNION ALL
SELECT o.[SolicitudId], o.[FechaCreacion], N'Orden de trabajo creada', CONCAT(N'Orden ', o.[Code], N'; estado: ', o.[Estado], N'.')
FROM [OrdenesTrabajo] o
UNION ALL
SELECT o.[SolicitudId], c.[Fecha], N'Cobro registrado', CONCAT(N'Factura ', c.[FacturaId], N'; estado: ', c.[Estado], N'; monto RD$ ', CONVERT(nvarchar(40), c.[Monto]), N'.')
FROM [Cobros] c INNER JOIN [OrdenesTrabajo] o ON o.[Id] = c.[OrdenTrabajoId];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesEventos");
        }
    }
}


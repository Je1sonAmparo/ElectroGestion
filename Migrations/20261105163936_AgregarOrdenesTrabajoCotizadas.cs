using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrdenesTrabajoCotizadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenesTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    CotizacionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoServicio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrabajosJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HerramientasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_CotizacionId",
                table: "OrdenesTrabajo",
                column: "CotizacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_SolicitudId",
                table: "OrdenesTrabajo",
                column: "SolicitudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesTrabajo");
        }
    }
}

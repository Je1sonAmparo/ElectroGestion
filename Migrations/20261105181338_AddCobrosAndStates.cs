using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AddCobrosAndStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cobros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacturaId = table.Column<int>(type: "int", nullable: false),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cobros_OrdenesTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 6, "En revisión" },
                    { 7, "Enviada al cliente" },
                    { 8, "Aprobada" },
                    { 9, "Rechazada" },
                    { 10, "Pendiente de cobro" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_OrdenTrabajoId",
                table: "Cobros",
                column: "OrdenTrabajoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cobros");

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}

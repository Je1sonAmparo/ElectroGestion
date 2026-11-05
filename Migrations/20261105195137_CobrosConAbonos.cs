using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class CobrosConAbonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cobros_OrdenTrabajoId",
                table: "Cobros");

            migrationBuilder.AlterColumn<int>(
                name: "OrdenTrabajoId",
                table: "Cobros",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CotizacionId",
                table: "Cobros",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE c SET CotizacionId = o.CotizacionId FROM Cobros c INNER JOIN OrdenesTrabajo o ON o.Id = c.OrdenTrabajoId");

            migrationBuilder.AlterColumn<int>(
                name: "CotizacionId",
                table: "Cobros",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AbonosCobro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CobroId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonosCobro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbonosCobro_Cobros_CobroId",
                        column: x => x.CobroId,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_CotizacionId",
                table: "Cobros",
                column: "CotizacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_OrdenTrabajoId",
                table: "Cobros",
                column: "OrdenTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobro_CobroId_Fecha",
                table: "AbonosCobro",
                columns: new[] { "CobroId", "Fecha" });

            migrationBuilder.Sql("INSERT INTO AbonosCobro (CobroId, Fecha, Monto, MetodoPago, Anulado) SELECT Id, Fecha, Monto, COALESCE(NULLIF(MetodoPago, ''), 'Transferencia'), 0 FROM Cobros WHERE Estado = 'Finalizada'");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobros_Cotizaciones_CotizacionId",
                table: "Cobros",
                column: "CotizacionId",
                principalTable: "Cotizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobros_Cotizaciones_CotizacionId",
                table: "Cobros");

            migrationBuilder.DropTable(
                name: "AbonosCobro");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_CotizacionId",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_OrdenTrabajoId",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "CotizacionId",
                table: "Cobros");

            migrationBuilder.AlterColumn<int>(
                name: "OrdenTrabajoId",
                table: "Cobros",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_OrdenTrabajoId",
                table: "Cobros",
                column: "OrdenTrabajoId",
                unique: true);
        }
    }
}

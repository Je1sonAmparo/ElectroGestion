using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class PagoMostradorYCodigoBarras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoRecibido",
                table: "Ventas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoBarra",
                table: "ArticulosInventario",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ArticulosInventario",
                keyColumn: "Id",
                keyValue: 1,
                column: "CodigoBarra",
                value: null);

            migrationBuilder.UpdateData(
                table: "ArticulosInventario",
                keyColumn: "Id",
                keyValue: 2,
                column: "CodigoBarra",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ArticulosInventario_CodigoBarra",
                table: "ArticulosInventario",
                column: "CodigoBarra",
                unique: true,
                filter: "[CodigoBarra] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticulosInventario_CodigoBarra",
                table: "ArticulosInventario");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MontoRecibido",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CodigoBarra",
                table: "ArticulosInventario");
        }
    }
}

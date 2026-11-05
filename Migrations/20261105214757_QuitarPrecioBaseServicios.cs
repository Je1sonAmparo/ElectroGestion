using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class QuitarPrecioBaseServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioBase",
                table: "Servicios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioBase",
                table: "Servicios",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PrecioBase",
                value: 85000m);

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PrecioBase",
                value: 45000m);

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: 3,
                column: "PrecioBase",
                value: 12500m);

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: 4,
                column: "PrecioBase",
                value: 28000m);

            migrationBuilder.UpdateData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: 5,
                column: "PrecioBase",
                value: 120000m);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVentasDirectas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloInventarioId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    CodigoArticulo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NombreArticulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentoCliente = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoComprobante = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ncf = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaItbis = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_ArticulosInventario_ArticuloInventarioId",
                        column: x => x.ArticuloInventarioId,
                        principalTable: "ArticulosInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ArticuloInventarioId",
                table: "Ventas",
                column: "ArticuloInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Estado_FechaVenta",
                table: "Ventas",
                columns: new[] { "Estado", "FechaVenta" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Ncf",
                table: "Ventas",
                column: "Ncf",
                unique: true,
                filter: "[Ncf] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}

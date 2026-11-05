using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class PersistirArticulosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticulosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticulosInventario", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ArticulosInventario",
                columns: new[] { "Id", "Cantidad", "Categoria", "Codigo", "Nombre", "PrecioUnidad" },
                values: new object[,]
                {
                    { 1, 500, "Materiales", "ART-001", "Cable de Cobre", 15m },
                    { 2, 10, "Equipos", "ART-002", "Transformador 50kVA", 25000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticulosInventario_Codigo",
                table: "ArticulosInventario",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticulosInventario");
        }
    }
}

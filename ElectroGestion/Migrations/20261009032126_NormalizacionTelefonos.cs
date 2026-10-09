using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class NormalizacionTelefonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesDirecciones");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Telefono2",
                table: "Clientes");

            migrationBuilder.CreateTable(
                name: "ClientesTelefonos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesTelefonos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesTelefonos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ClientesTelefonos",
                columns: new[] { "Id", "ClienteId", "Numero", "Tipo" },
                values: new object[,]
                {
                    { 1, 1, "809-000-0000", "Principal" },
                    { 2, 1, "829-111-2222", "Alternativo" },
                    { 3, 2, "809-555-1212", "Principal" },
                    { 4, 3, "829-441-8890", "Principal" },
                    { 5, 4, "849-220-3311", "Principal" },
                    { 6, 5, "809-778-4402", "Principal" },
                    { 7, 5, "809-999-0000", "Alternativo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTelefonos_ClienteId",
                table: "ClientesTelefonos",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesTelefonos");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono2",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClientesDirecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesDirecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesDirecciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Telefono", "Telefono2" },
                values: new object[] { "809-000-0000", "829-111-2222" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Telefono", "Telefono2" },
                values: new object[] { "809-555-1212", "" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Telefono", "Telefono2" },
                values: new object[] { "829-441-8890", "" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Telefono", "Telefono2" },
                values: new object[] { "849-220-3311", "" });

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Telefono", "Telefono2" },
                values: new object[] { "809-778-4402", "809-999-0000" });

            migrationBuilder.InsertData(
                table: "ClientesDirecciones",
                columns: new[] { "Id", "ClienteId", "Direccion", "Tipo" },
                values: new object[,]
                {
                    { 1, 1, "Calle Duarte #24, Santiago", "Principal" },
                    { 2, 2, "Av. Estrella Sadhalá #102, Santiago", "Principal" },
                    { 3, 3, "Calle 16 #7, La Vega", "Principal" },
                    { 4, 4, "Calle Principal #58, Moca", "Principal" },
                    { 5, 5, "Carretera Luperón km 5, Santiago", "Principal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesDirecciones_ClienteId",
                table: "ClientesDirecciones",
                column: "ClienteId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioLogin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Cargo", "Nombre", "PasswordHash", "Rol", "UsuarioLogin" },
                values: new object[,]
                {
                    { 1, "Gerente", "Ana Hernández", "AQAAAAIAAYagAAAAEJvgEPmcnhCOwOXMhDWnexFdwrQh5Mp/b03A6SZkGM8aoTtqxVaYaiMdDG4D1BgbqQ==", "Gerente", "gerente" },
                    { 2, "Técnico", "Luis Peña", "AQAAAAIAAYagAAAAEPeGfF62+i6tlvwb/sGQEx4ZyM4cP0sxOuSW3LIBCB8siO0FC0OAM/bXUzKQK/ujqw==", "Técnico", "tecnico" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuarioLogin",
                table: "Usuarios",
                column: "UsuarioLogin",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}

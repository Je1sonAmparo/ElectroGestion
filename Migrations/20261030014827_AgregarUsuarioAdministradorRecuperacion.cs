using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarioAdministradorRecuperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EliminadaPorSolicitud",
                table: "Visitas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE v SET EliminadaPorSolicitud = 1 FROM Visitas v INNER JOIN Solicitudes s ON s.Id = v.SolicitudId WHERE v.IsDeleted = 1 AND s.IsDeleted = 1");

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Cargo", "Nombre", "PasswordHash", "Rol", "UsuarioLogin" },
                values: new object[] { 3, "Administrador", "Administrador", "AQAAAAIAAYagAAAAEDlXkB6f7Cn7W7LhwqonLz2YWMWnkh0z783OtK2aOGgJYxTR1gP7p26z3JILLccOWQ==", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "EliminadaPorSolicitud",
                table: "Visitas");
        }
    }
}

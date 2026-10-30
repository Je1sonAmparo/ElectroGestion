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

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioLogin = N'admin')
                BEGIN
                    DECLARE @AdminId int;
                    SELECT @AdminId = ISNULL(MAX(Id), 0) + 1 FROM dbo.Usuarios;

                    SET IDENTITY_INSERT dbo.Usuarios ON;
                    INSERT INTO dbo.Usuarios (Id, Cargo, Nombre, PasswordHash, Rol, UsuarioLogin)
                    VALUES (@AdminId, N'Administrador', N'Administrador', N'AQAAAAIAAYagAAAAEDlXkB6f7Cn7W7LhwqonLz2YWMWnkh0z783OtK2aOGgJYxTR1gP7p26z3JILLccOWQ==', N'Admin', N'admin');
                    SET IDENTITY_INSERT dbo.Usuarios OFF;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.Usuarios WHERE UsuarioLogin = N'admin'");

            migrationBuilder.DropColumn(
                name: "EliminadaPorSolicitud",
                table: "Visitas");
        }
    }
}

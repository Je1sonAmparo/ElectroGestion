using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class UsarNombresComoCredencialesDeUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "UsuarioLogin",
                value: "Ana Hernández");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "UsuarioLogin",
                value: "Luis Peña");

            migrationBuilder.Sql("""
                UPDATE [Usuarios]
                SET [UsuarioLogin] = N'Carlos Méndez'
                WHERE [UsuarioLogin] = N'carlos'
                    AND [Nombre] = N'Carlos Méndez'
                    AND [Rol] = N'Técnico';

                IF @@ROWCOUNT <> 1
                    THROW 51005, N'No se pudo actualizar el nombre de usuario de Carlos Méndez.', 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Usuarios]
                SET [UsuarioLogin] = N'carlos'
                WHERE [UsuarioLogin] = N'Carlos Méndez'
                    AND [Nombre] = N'Carlos Méndez'
                    AND [Rol] = N'Técnico';
                """);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "UsuarioLogin",
                value: "gerente");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "UsuarioLogin",
                value: "tecnico");
        }
    }
}

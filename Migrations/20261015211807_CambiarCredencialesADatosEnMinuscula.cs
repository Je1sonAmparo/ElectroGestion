using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class CambiarCredencialesADatosEnMinuscula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "UsuarioLogin" },
                values: new object[] { "AQAAAAIAAYagAAAAEHjAvVupymZcL4Z7wSkz9ro//N3Hyz31400GKF9GEPnxQFaOPv8TGpO2FJZ8XqCSwg==", "ana" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "UsuarioLogin" },
                values: new object[] { "AQAAAAIAAYagAAAAEAXFkAX9eqMt9778g9tjjfmZCQWcmM5qlxzOI9VP6eXkDBo7XL52AGtJ9CTZWHUm7g==", "luis" });

            migrationBuilder.Sql("""
                UPDATE [Usuarios]
                SET [UsuarioLogin] = N'carlos',
                    [PasswordHash] = N'AQAAAAIAAYagAAAAEJ4WEd0KMz+I+rffx28FYgb2hvYECf346n9pQM4tprVvkthBI0p9NSKYFGpC9uNsSg=='
                WHERE [UsuarioLogin] = N'Carlos Méndez'
                    AND [Nombre] = N'Carlos Méndez'
                    AND [Rol] = N'Técnico';

                IF @@ROWCOUNT <> 1
                    THROW 51006, N'No se pudieron actualizar las credenciales de Carlos Méndez.', 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Usuarios]
                SET [UsuarioLogin] = N'Carlos Méndez',
                    [PasswordHash] = N'AQAAAAIAAYagAAAAEMB/vjFzmu7a6+6obTRsJEzOzWZkBqfG9Sy1qVmJ/d1f0CP61CbwqUdTOQM0lzd+pQ=='
                WHERE [UsuarioLogin] = N'carlos'
                    AND [Nombre] = N'Carlos Méndez'
                    AND [Rol] = N'Técnico';
                """);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "UsuarioLogin" },
                values: new object[] { "AQAAAAIAAYagAAAAEJvgEPmcnhCOwOXMhDWnexFdwrQh5Mp/b03A6SZkGM8aoTtqxVaYaiMdDG4D1BgbqQ==", "Ana Hernández" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "UsuarioLogin" },
                values: new object[] { "AQAAAAIAAYagAAAAEPeGfF62+i6tlvwb/sGQEx4ZyM4cP0sxOuSW3LIBCB8siO0FC0OAM/bXUzKQK/ujqw==", "Luis Peña" });
        }
    }
}

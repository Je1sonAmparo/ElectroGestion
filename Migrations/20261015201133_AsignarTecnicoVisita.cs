using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AsignarTecnicoVisita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TecnicoId",
                table: "Visitas",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE v
                SET TecnicoId = u.Id
                FROM Visitas v
                INNER JOIN Solicitudes s ON s.Id = v.SolicitudId
                INNER JOIN Usuarios u ON LTRIM(RTRIM(u.Nombre)) = LTRIM(RTRIM(s.TecnicoAsignado))
                    AND u.Rol = N'Técnico'
                WHERE LTRIM(RTRIM(s.TecnicoAsignado)) <> N'';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_TecnicoId",
                table: "Visitas",
                column: "TecnicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Usuarios_TecnicoId",
                table: "Visitas",
                column: "TecnicoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Usuarios_TecnicoId",
                table: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Visitas_TecnicoId",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "TecnicoId",
                table: "Visitas");

        }
    }
}

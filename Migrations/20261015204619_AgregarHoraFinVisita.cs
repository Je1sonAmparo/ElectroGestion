using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHoraFinVisita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HoraFin",
                table: "Visitas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Visitas
                SET HoraFin = COALESCE(
                    CONVERT(varchar(5), DATEADD(MINUTE, 150, TRY_CONVERT(time(0), Hora)), 108),
                    Hora)
                WHERE HoraFin = N'';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraFin",
                table: "Visitas");
        }
    }
}

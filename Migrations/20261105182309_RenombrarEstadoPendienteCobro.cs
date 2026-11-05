using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarEstadoPendienteCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Cobros] SET [Estado] = N'Pendiente de cobro' WHERE [Estado] = N'Pendiente'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Cobros] SET [Estado] = N'Pendiente' WHERE [Estado] = N'Pendiente de cobro'");
        }
    }
}

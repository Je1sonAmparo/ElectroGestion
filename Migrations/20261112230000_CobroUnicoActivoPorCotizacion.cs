using ElectroGestion.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations;

[DbContext(typeof(Contexto))]
[Migration("20261112230000_CobroUnicoActivoPorCotizacion")]
public partial class CobroUnicoActivoPorCotizacion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Cobros_CotizacionId", table: "Cobros");
        migrationBuilder.CreateIndex(
            name: "IX_Cobros_CotizacionId",
            table: "Cobros",
            column: "CotizacionId",
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Cobros_CotizacionId", table: "Cobros");
        migrationBuilder.CreateIndex(
            name: "IX_Cobros_CotizacionId",
            table: "Cobros",
            column: "CotizacionId",
            unique: true);
    }
}

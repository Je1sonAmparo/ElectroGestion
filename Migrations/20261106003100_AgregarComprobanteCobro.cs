using ElectroGestion.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations;

[DbContext(typeof(Contexto))]
[Migration("20261106003100_AgregarComprobanteCobro")]
public partial class AgregarComprobanteCobro : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TipoComprobante",
            table: "Cobros",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "Consumidor final");

        migrationBuilder.AddColumn<string>(
            name: "DocumentoCliente",
            table: "Cobros",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Ncf",
            table: "Cobros",
            type: "nvarchar(13)",
            maxLength: 13,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "TipoComprobante", table: "Cobros");
        migrationBuilder.DropColumn(name: "DocumentoCliente", table: "Cobros");
        migrationBuilder.DropColumn(name: "Ncf", table: "Cobros");
    }
}
